using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;
using University_Management_System.Services;

namespace University_Management_System.Controllers.StudentManagement
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize (Roles = SystemRoles.superAdmin)]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailService _emailService;

        public StudentController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }


        //Post API for User

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

            // Check whether AcademicProgram exists
            if (model.AcademicProgramId != null)
            {
                var programExist = await _context.AcademicPrograms.AnyAsync(p => p.Id == model.AcademicProgramId);
                if (!programExist)
                {
                    return BadRequest(new { message = "Academic program not found" });
                }
            }

            // Create initial Student with Inactive status and no Email/UserId
            var student = new Student
            {
                StudentName = model.StudentName,
                RollNumber = model.RollNumber,
                DepartmentId = model.DepartmentId,
                AcademicProgramId = model.AcademicProgramId,
                Semester = model.Semester,
                Status = "Inactive",
                Email = null,
                UserId = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Create RegistrationInvitation and send invitation email
            if (!string.IsNullOrWhiteSpace(model.InvitationEmail))
            {
                var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

                var invitation = new RegistrationInvitation
                {
                    Token = token,
                    Role = SystemRoles.Student,
                    ProfileId = student.Id,
                    RecipientEmail = model.InvitationEmail,
                    ExpiresAt = DateTime.UtcNow.AddDays(2),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RegistrationInvitations.Add(invitation);
                await _context.SaveChangesAsync();

                var registrationLink = $"http://localhost:5173/signUp?token={token}";

                var subject = "University Student Registration Invitation";
                var body = $@"
                    <p>Your student account has been created by the university.</p>
                    <p>Please use the following link to complete your registration:</p>
                    <p><a href='{registrationLink}'>{registrationLink}</a></p>
                ";

                try
                {
                    await _emailService.SendEmailAsync(model.InvitationEmail, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email sending failed: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "Student Added Successfully",
                studentId = student.Id
            });
        }



        //Get Api

        [HttpGet]
        public async Task <IActionResult> GetStudents()
        {
            var students =await  _context.Students
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
                    student.Semester,
                    student.Status
                })
                .ToListAsync();

            return Ok(students);
        }


        //Get Student by Id

        [HttpGet("{id}")]
        public IActionResult GetStudentById(int id)
        {
            var student = _context.Students
                .FirstOrDefault(s => s.Id == id);

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

            student.StudentName = model.StudentName;
            student.RollNumber = model.RollNumber;
            student.DepartmentId = model.DepartmentId;
            student.AcademicProgramId = model.AcademicProgramId;
            student.Semester = model.Semester;

            //Check whether the Roll Number given by admin is already assigned to any student or not 
            var duplicateRollNumber = await _context.Students
                .AnyAsync(s => s.RollNumber == model.RollNumber && s.Id != id);

            if (duplicateRollNumber)
            {
                return BadRequest(new { message = "Roll number already exists" });
            }

            //Check whether the department Id given by the user exists in Department table
            if (model.DepartmentId != null)
            {
                var departmentExists = await _context.Departments
                    .AnyAsync(d => d.Id == model.DepartmentId);

                if (!departmentExists)
                {
                    return BadRequest(new { message = "Department does not exist" });
                }
            }

            //Check whether the academic program exists in AcademicProgram table
            if (model.AcademicProgramId != null)
            {
                var programExists = await _context.AcademicPrograms
                    .AnyAsync(p => p.Id == model.AcademicProgramId);

                if (!programExists)
                {
                    return BadRequest(new { message = "Academic program does not exist" });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Student updated successfully" });
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
        public async Task <IActionResult> DeleteStudent(int id)
        {
            var student = _context.Students
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

       



            var user = await _userManager.FindByIdAsync(student.UserId!);

            // Identity user delete karo
            if (user != null)
            {
                var userResult = await _userManager.DeleteAsync(user);

                if (!userResult.Succeeded)
                {
                    return BadRequest(userResult.Errors);
                }
            }

            // Students table ki row delete karo
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Student deleted successfully" });
        }
    }
}
