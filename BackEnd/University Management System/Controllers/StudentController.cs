using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models;
using University_Management_System.Services;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize (Policy ="Admin Access")]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailService _emailService;
        private readonly EnrollmentSyncService _enrollmentSyncService;

        public StudentController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            EmailService emailService,
            EnrollmentSyncService enrollmentSyncService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _enrollmentSyncService = enrollmentSyncService;
        }


        //Post API for Student
        [HttpPost]
        public async Task<IActionResult> AddStudent([FromBody] AddStudentDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.StudentName))
            {
                return BadRequest(new { message = "Student name is required" });
            }

            if (string.IsNullOrWhiteSpace(model.RollNumber))
            {
                return BadRequest(new { message = "Roll number is required" });
            }

            // Check duplication of RollNumber
            var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.RollNumber == model.RollNumber);
            if (existingStudent != null)
            {
                return BadRequest(new { message = "Roll number already exists" });
            }

            // Check whether Department exists
            if (model.DepartmentId != null)
            {
                var departmentExist = await _context.Departments.AnyAsync(d => d.Id == model.DepartmentId);
                if (!departmentExist)
                {
                    return BadRequest(new { message = "Department not found" });
                }
            }

            // Check whether AcademicProgram exists and matches Department
            AcademicProgram? program = null;
            if (model.AcademicProgramId != null)
            {
                program = await _context.AcademicPrograms.FirstOrDefaultAsync(p => p.Id == model.AcademicProgramId);
                if (program == null)
                {
                    return BadRequest(new { message = "Academic program not found" });
                }

                if (model.DepartmentId != null && program.DepartmentId != model.DepartmentId)
                {
                    return BadRequest(new { message = "Selected academic program does not belong to the selected department" });
                }
            }

            // Check Semester and validations
            Semester? semester = null;
            if (model.CurrentSemesterId.HasValue)
            {
                semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == model.CurrentSemesterId.Value);
                if (semester == null)
                {
                    return BadRequest(new { message = "Selected semester does not exist" });
                }

                if (semester.Status != null && semester.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Selected semester is inactive" });
                }

                if (model.AcademicProgramId.HasValue && semester.AcademicProgramId != model.AcademicProgramId.Value)
                {
                    return BadRequest(new { message = "Selected semester does not belong to the selected academic program" });
                }

                if (model.AcademicSessionId.HasValue && semester.AcademicSessionId != model.AcademicSessionId.Value)
                {
                    return BadRequest(new { message = "Selected semester does not belong to the selected academic session" });
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create initial Student with Inactive status and no Email/UserId
                var student = new Student
                {
                    StudentName = model.StudentName.Trim(),
                    RollNumber = model.RollNumber.Trim(),
                    DepartmentId = model.DepartmentId,
                    AcademicProgramId = model.AcademicProgramId,
                    CurrentSemesterId = semester?.Id,
                    Semester = semester?.SemesterNumber ?? model.Semester,
                    Status = "Inactive",
                    Email = null,
                    UserId = null,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // Auto enroll into all active course offerings for the assigned semester via EnrollmentSyncService
                int enrollmentsCreated = 0;
                if (student.CurrentSemesterId.HasValue)
                {
                    enrollmentsCreated = await _enrollmentSyncService.SynchronizeSemesterEnrollmentsAsync(student.CurrentSemesterId.Value);
                }

                // Create RegistrationInvitation if email is provided
                RegistrationInvitation? invitation = null;
                if (!string.IsNullOrWhiteSpace(model.InvitationEmail))
                {
                    var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

                    invitation = new RegistrationInvitation
                    {
                        Token = token,
                        Role = SystemRoles.Student,
                        ProfileId = student.Id,
                        RecipientEmail = model.InvitationEmail.Trim(),
                        ExpiresAt = DateTime.UtcNow.AddDays(2),
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.RegistrationInvitations.Add(invitation);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Post-commit email dispatch
                if (invitation != null)
                {
                    var registrationLink = $"http://localhost:5173/signUp?token={invitation.Token}";
                    var subject = "University Student Registration Invitation";
                    var body = $@"
                        <p>Your student account has been created by the university.</p>
                        <p>Please use the following link to complete your registration:</p>
                        <p><a href='{registrationLink}'>{registrationLink}</a></p>
                    ";

                    try
                    {
                        await _emailService.SendEmailAsync(invitation.RecipientEmail, subject, body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Email sending failed: {ex.Message}");
                    }
                }

                var successMessage = enrollmentsCreated > 0
                    ? $"Student added successfully with {enrollmentsCreated} active course enrollment(s)."
                    : "Student added successfully. No active course offerings found for this semester yet.";

                return Ok(new
                {
                    message = successMessage,
                    studentId = student.Id,
                    enrollmentsCount = enrollmentsCreated
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred while creating the student.", error = ex.Message });
            }
        }

        //Get Api
        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Students
                .Select(student => new
                {
                    student.Id,
                    student.UserId,
                    student.Email,
                    student.StudentName,
                    student.RollNumber,
                    student.DepartmentId,
                    DepartmentName = student.Department != null
                        ? student.Department.DepartmentName
                        : null,
                    student.AcademicProgramId,
                    ProgramName = student.AcademicProgram != null
                        ? student.AcademicProgram.ProgramName
                        : null,
                    student.CurrentSemesterId,
                    SemesterNumber = student.CurrentSemester != null
                        ? student.CurrentSemester.SemesterNumber
                        : student.Semester,
                    AcademicSessionId = student.CurrentSemester != null
                        ? (int?)student.CurrentSemester.AcademicSessionId
                        : null,
                    SessionName = student.CurrentSemester != null && student.CurrentSemester.AcademicSession != null
                        ? student.CurrentSemester.AcademicSession.SessionName
                        : null,
                    student.Semester,
                    student.Status,
                    student.CreatedAt
                })
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            return Ok(students);
        }

        //Get Student by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _context.Students
                .Where(s => s.Id == id)
                .Select(student => new
                {
                    student.Id,
                    student.UserId,
                    student.Email,
                    student.StudentName,
                    student.RollNumber,
                    student.DepartmentId,
                    DepartmentName = student.Department != null
                        ? student.Department.DepartmentName
                        : null,
                    student.AcademicProgramId,
                    ProgramName = student.AcademicProgram != null
                        ? student.AcademicProgram.ProgramName
                        : null,
                    student.CurrentSemesterId,
                    SemesterNumber = student.CurrentSemester != null
                        ? student.CurrentSemester.SemesterNumber
                        : student.Semester,
                    AcademicSessionId = student.CurrentSemester != null
                        ? (int?)student.CurrentSemester.AcademicSessionId
                        : null,
                    SessionName = student.CurrentSemester != null && student.CurrentSemester.AcademicSession != null
                        ? student.CurrentSemester.AcademicSession.SessionName
                        : null,
                    student.Semester,
                    student.Status,
                    student.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            return Ok(student);
        }

        //Update the student Detail
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] AddStudentDTO model)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            if (string.IsNullOrWhiteSpace(model.StudentName))
            {
                return BadRequest(new { message = "Student name is required" });
            }

            if (string.IsNullOrWhiteSpace(model.RollNumber))
            {
                return BadRequest(new { message = "Roll number is required" });
            }

            // Check duplicate RollNumber
            var duplicateRollNumber = await _context.Students
                .AnyAsync(s => s.RollNumber == model.RollNumber && s.Id != id);

            if (duplicateRollNumber)
            {
                return BadRequest(new { message = "Roll number already exists" });
            }

            // Check Department
            if (model.DepartmentId != null)
            {
                var departmentExists = await _context.Departments
                    .AnyAsync(d => d.Id == model.DepartmentId);

                if (!departmentExists)
                {
                    return BadRequest(new { message = "Department does not exist" });
                }
            }

            // Check Academic Program
            AcademicProgram? program = null;
            if (model.AcademicProgramId != null)
            {
                program = await _context.AcademicPrograms
                    .FirstOrDefaultAsync(p => p.Id == model.AcademicProgramId);

                if (program == null)
                {
                    return BadRequest(new { message = "Academic program does not exist" });
                }

                if (model.DepartmentId != null && program.DepartmentId != model.DepartmentId)
                {
                    return BadRequest(new { message = "Selected academic program does not belong to the selected department" });
                }
            }

            // Check Semester
            Semester? semester = null;
            if (model.CurrentSemesterId.HasValue)
            {
                semester = await _context.Semesters
                    .FirstOrDefaultAsync(s => s.Id == model.CurrentSemesterId.Value);

                if (semester == null)
                {
                    return BadRequest(new { message = "Selected semester does not exist" });
                }

                if (semester.Status != null && semester.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Selected semester is inactive" });
                }

                if (model.AcademicProgramId.HasValue && semester.AcademicProgramId != model.AcademicProgramId.Value)
                {
                    return BadRequest(new { message = "Selected semester does not belong to the selected academic program" });
                }

                if (model.AcademicSessionId.HasValue && semester.AcademicSessionId != model.AcademicSessionId.Value)
                {
                    return BadRequest(new { message = "Selected semester does not belong to the selected academic session" });
                }
            }

            int? oldSemesterId = student.CurrentSemesterId;

            student.StudentName = model.StudentName.Trim();
            student.RollNumber = model.RollNumber.Trim();
            student.DepartmentId = model.DepartmentId;
            student.AcademicProgramId = model.AcademicProgramId;
            if (semester != null)
            {
                student.CurrentSemesterId = semester.Id;
                student.Semester = semester.SemesterNumber;
            }
            else if (model.Semester.HasValue)
            {
                student.Semester = model.Semester.Value;
            }

            await _context.SaveChangesAsync();

            // If semester changed or was newly assigned, enroll student in active offerings for the new semester via EnrollmentSyncService
            int newlyEnrolledCount = 0;
            if (student.CurrentSemesterId.HasValue && student.CurrentSemesterId != oldSemesterId)
            {
                newlyEnrolledCount = await _enrollmentSyncService.SynchronizeSemesterEnrollmentsAsync(student.CurrentSemesterId.Value);
            }

            var updateMessage = newlyEnrolledCount > 0
                ? $"Student updated successfully and enrolled in {newlyEnrolledCount} new course(s)."
                : "Student updated successfully";

            return Ok(new { message = updateMessage });
        }

        //Update Student Status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStudentStatus(int id, [FromBody] UpdateStudentStatusDTO model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Status))
            {
                return BadRequest(new { message = "Status is required" });
            }

            var newStatus = model.Status.Trim();
            if (newStatus != "Active" && newStatus != "Inactive")
            {
                return BadRequest(new { message = "Invalid status. Status must be 'Active' or 'Inactive'" });
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            if (newStatus == "Active" && (string.IsNullOrEmpty(student.UserId) || string.IsNullOrEmpty(student.Email)))
            {
                return BadRequest(new { message = "Cannot activate student who has not completed registration." });
            }

            student.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Student status updated to {newStatus} successfully", status = newStatus });
        }

        //Delete Student API
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            if (!string.IsNullOrEmpty(student.UserId))
            {
                var user = await _userManager.FindByIdAsync(student.UserId);
                if (user != null)
                {
                    var userResult = await _userManager.DeleteAsync(user);
                    if (!userResult.Succeeded)
                    {
                        return BadRequest(userResult.Errors);
                    }
                }
            }

            var invitations = await _context.RegistrationInvitations
                .Where(i => i.ProfileId == student.Id && i.Role == SystemRoles.Student)
                .ToListAsync();
            if (invitations.Any())
            {
                _context.RegistrationInvitations.RemoveRange(invitations);
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .ToListAsync();
            if (enrollments.Any())
            {
                _context.Enrollments.RemoveRange(enrollments);
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Student deleted successfully" });
        }
    }
}


