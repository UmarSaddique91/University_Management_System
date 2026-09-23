using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.SemesterManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SystemRoles.superAdmin},{SystemRoles.Admin},{SystemRoles.Registrar}")]
    public class SemesterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SemesterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Semester?programId=1&sessionId=2
        [HttpGet]
        public async Task<IActionResult> GetSemesters([FromQuery] int? programId, [FromQuery] int? sessionId)
        {
            var query = _context.Semesters.AsQueryable();

            if (programId.HasValue && programId.Value > 0)
            {
                query = query.Where(s => s.AcademicProgramId == programId.Value);
            }

            if (sessionId.HasValue && sessionId.Value > 0)
            {
                query = query.Where(s => s.AcademicSessionId == sessionId.Value);
            }

            var semesters = await query
                .OrderBy(s => s.SemesterNumber)
                .Select(s => new
                {
                    s.Id,
                    s.SemesterNumber,
                    s.AcademicProgramId,
                    ProgramName = s.AcademicProgram != null ? s.AcademicProgram.ProgramName : null,
                    ProgramCode = s.AcademicProgram != null ? s.AcademicProgram.ProgramCode : null,
                    DurationInSemesters = s.AcademicProgram != null ? s.AcademicProgram.DurationInSemesters : 8,
                    s.AcademicSessionId,
                    SessionName = s.AcademicSession != null ? s.AcademicSession.SessionName : null,
                    Status = s.Status ?? "Active",

                    // Student Count: current Students assigned to that Semester
                    StudentCount = _context.Students
                        .Count(st => st.CurrentSemesterId == s.Id),

                    // Enrolled Students Count: distinct students enrolled through Active CourseOfferings
                    ActiveEnrolledStudentsCount = _context.Enrollments
                        .Where(e => e.CourseOffering.SemesterId == s.Id && e.Status == "Active" && e.CourseOffering.Status == "Active")
                        .Select(e => e.StudentId)
                        .Distinct()
                        .Count(),

                    // Profile-based student count exposed separately
                    ProfileStudentCount = _context.Students
                        .Count(st => st.AcademicProgramId == s.AcademicProgramId && st.Semester == s.SemesterNumber),

                    // Assigned Teachers Count: distinct teachers from Active CourseOfferings who are themselves Active
                    AssignedTeachersCount = _context.CourseOfferings
                        .Where(co => co.SemesterId == s.Id && co.Status == "Active" && co.Teacher != null && co.Teacher.Status == "Active")
                        .Select(co => co.TeacherId)
                        .Distinct()
                        .Count(),

                    // Courses Count: distinct courses offered in this Semester via active CourseOfferings
                    CourseCount = _context.CourseOfferings
                        .Where(co => co.SemesterId == s.Id && co.Status == "Active")
                        .Select(co => co.CourseId)
                        .Distinct()
                        .Count(),

                    // Comprehensive Dependency Detection
                    HasDependencies = _context.Students.Any(st => st.CurrentSemesterId == s.Id) ||
                                      _context.CourseOfferings.Any(co => co.SemesterId == s.Id) ||
                                      _context.Enrollments.Any(e => e.CourseOffering.SemesterId == s.Id) ||
                                      _context.Attendance.Any(a => a.CourseOffering.SemesterId == s.Id) ||
                                      _context.Exams.Any(ex => ex.CourseOffering.SemesterId == s.Id) ||
                                      _context.Grades.Any(g => g.Exam.CourseOffering.SemesterId == s.Id)
                })
                .ToListAsync();

            return Ok(semesters);
        }

        // GET: api/Semester/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSemesterById(int id)
        {
            var semester = await _context.Semesters
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.SemesterNumber,
                    s.AcademicProgramId,
                    ProgramName = s.AcademicProgram != null ? s.AcademicProgram.ProgramName : null,
                    ProgramCode = s.AcademicProgram != null ? s.AcademicProgram.ProgramCode : null,
                    DurationInSemesters = s.AcademicProgram != null ? s.AcademicProgram.DurationInSemesters : 8,
                    s.AcademicSessionId,
                    SessionName = s.AcademicSession != null ? s.AcademicSession.SessionName : null,
                    Status = s.Status ?? "Active",

                    // Student Count: current Students assigned to that Semester
                    StudentCount = _context.Students
                        .Count(st => st.CurrentSemesterId == s.Id),

                    // Enrolled Students Count: distinct students enrolled through Active CourseOfferings
                    ActiveEnrolledStudentsCount = _context.Enrollments
                        .Where(e => e.CourseOffering.SemesterId == s.Id && e.Status == "Active" && e.CourseOffering.Status == "Active")
                        .Select(e => e.StudentId)
                        .Distinct()
                        .Count(),

                    ProfileStudentCount = _context.Students
                        .Count(st => st.AcademicProgramId == s.AcademicProgramId && st.Semester == s.SemesterNumber),

                    AssignedTeachersCount = _context.CourseOfferings
                        .Where(co => co.SemesterId == s.Id && co.Status == "Active" && co.Teacher != null && co.Teacher.Status == "Active")
                        .Select(co => co.TeacherId)
                        .Distinct()
                        .Count(),

                    CourseCount = _context.CourseOfferings
                        .Where(co => co.SemesterId == s.Id && co.Status == "Active")
                        .Select(co => co.CourseId)
                        .Distinct()
                        .Count(),

                    HasDependencies = _context.Students.Any(st => st.CurrentSemesterId == s.Id) ||
                                      _context.CourseOfferings.Any(co => co.SemesterId == s.Id) ||
                                      _context.Enrollments.Any(e => e.CourseOffering.SemesterId == s.Id) ||
                                      _context.Attendance.Any(a => a.CourseOffering.SemesterId == s.Id) ||
                                      _context.Exams.Any(ex => ex.CourseOffering.SemesterId == s.Id) ||
                                      _context.Grades.Any(g => g.Exam.CourseOffering.SemesterId == s.Id)
                })
                .FirstOrDefaultAsync();

            if (semester == null)
            {
                return NotFound(new { message = "Semester not found" });
            }

            return Ok(semester);
        }

        // POST: api/Semester
        [HttpPost]
        public async Task<IActionResult> AddSemester([FromBody] SemesterDTO model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Semester data is required." });
            }

            var program = await _context.AcademicPrograms.FirstOrDefaultAsync(p => p.Id == model.AcademicProgramId);
            if (program == null)
            {
                return BadRequest(new { message = "Selected academic program does not exist." });
            }

            var session = await _context.AcademicSessions.FirstOrDefaultAsync(s => s.Id == model.AcademicSessionId);
            if (session == null)
            {
                return BadRequest(new { message = "Selected academic session does not exist." });
            }

            int maxSemesters = program.DurationInSemesters.HasValue && program.DurationInSemesters.Value > 0
                ? program.DurationInSemesters.Value
                : 8;

            if (model.SemesterNumber <= 0 || model.SemesterNumber > maxSemesters)
            {
                return BadRequest(new
                {
                    message = $"Semester number must be between 1 and {maxSemesters} for {program.ProgramName}."
                });
            }

            var exists = await _context.Semesters.AnyAsync(s =>
                s.AcademicProgramId == model.AcademicProgramId &&
                s.AcademicSessionId == model.AcademicSessionId &&
                s.SemesterNumber == model.SemesterNumber);

            if (exists)
            {
                return BadRequest(new
                {
                    message = $"Semester {model.SemesterNumber} already exists for {program.ProgramName} in {session.SessionName}."
                });
            }

            var semester = new Semester
            {
                SemesterNumber = model.SemesterNumber,
                AcademicProgramId = model.AcademicProgramId,
                AcademicSessionId = model.AcademicSessionId,
                Status = !string.IsNullOrWhiteSpace(model.Status) ? model.Status.Trim() : "Active"
            };

            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Semester added successfully",
                semester.Id
            });
        }

        // PUT: api/Semester/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSemester(int id, [FromBody] SemesterDTO model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Semester data is required." });
            }

            var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == id);
            if (semester == null)
            {
                return NotFound(new { message = "Semester not found" });
            }

            var program = await _context.AcademicPrograms.FirstOrDefaultAsync(p => p.Id == semester.AcademicProgramId);
            int maxSemesters = program?.DurationInSemesters.HasValue == true && program.DurationInSemesters.Value > 0
                ? program.DurationInSemesters.Value
                : 8;

            // Check dependencies
            var hasDependencies = await _context.CourseOfferings.AnyAsync(co => co.SemesterId == id) ||
                                  await _context.Enrollments.AnyAsync(e => e.CourseOffering.SemesterId == id) ||
                                  await _context.Attendance.AnyAsync(a => a.CourseOffering.SemesterId == id) ||
                                  await _context.Exams.AnyAsync(ex => ex.CourseOffering.SemesterId == id) ||
                                  await _context.Grades.AnyAsync(g => g.Exam.CourseOffering.SemesterId == id);

            if (hasDependencies)
            {
                if (model.SemesterNumber != semester.SemesterNumber || model.AcademicSessionId != semester.AcademicSessionId)
                {
                    return BadRequest(new
                    {
                        message = "Semester number and Academic Session cannot be modified because dependent course offerings or academic records exist. Only status can be updated."
                    });
                }
            }
            else
            {
                if (model.SemesterNumber <= 0 || model.SemesterNumber > maxSemesters)
                {
                    return BadRequest(new
                    {
                        message = $"Semester number must be between 1 and {maxSemesters}."
                    });
                }

                var sessionExists = await _context.AcademicSessions.AnyAsync(s => s.Id == model.AcademicSessionId);
                if (!sessionExists)
                {
                    return BadRequest(new { message = "Academic session does not exist." });
                }

                var duplicate = await _context.Semesters.AnyAsync(s =>
                    s.AcademicProgramId == semester.AcademicProgramId &&
                    s.AcademicSessionId == model.AcademicSessionId &&
                    s.SemesterNumber == model.SemesterNumber &&
                    s.Id != id);

                if (duplicate)
                {
                    return BadRequest(new { message = $"Semester {model.SemesterNumber} already exists in this session." });
                }

                semester.SemesterNumber = model.SemesterNumber;
                semester.AcademicSessionId = model.AcademicSessionId;
            }

            if (!string.IsNullOrWhiteSpace(model.Status))
            {
                semester.Status = model.Status.Trim();
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Semester updated successfully" });
        }

        // PATCH: api/Semester/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] StatusUpdateDTO? model)
        {
            var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == id);
            if (semester == null)
            {
                return NotFound(new { message = "Semester not found" });
            }

            var nextStatus = !string.IsNullOrWhiteSpace(model?.Status)
                ? model.Status.Trim()
                : (semester.Status == "Active" ? "Inactive" : "Active");

            semester.Status = nextStatus;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Semester status updated to {nextStatus}",
                status = nextStatus
            });
        }

        // DELETE: api/Semester/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSemester(int id)
        {
            var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == id);
            if (semester == null)
            {
                return NotFound(new { message = "Semester not found" });
            }

            // Safe-delete check: Inspect all linked academic records
            var hasStudents = await _context.Students.AnyAsync(st => st.CurrentSemesterId == id);
            var hasOfferings = await _context.CourseOfferings.AnyAsync(co => co.SemesterId == id);
            var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.CourseOffering.SemesterId == id);
            var hasAttendance = await _context.Attendance.AnyAsync(a => a.CourseOffering.SemesterId == id);
            var hasExams = await _context.Exams.AnyAsync(ex => ex.CourseOffering.SemesterId == id);
            var hasGrades = await _context.Grades.AnyAsync(g => g.Exam.CourseOffering.SemesterId == id);

            if (hasStudents || hasOfferings || hasEnrollments || hasAttendance || hasExams || hasGrades)
            {
                return BadRequest(new
                {
                    message = $"Cannot delete Semester {semester.SemesterNumber} because it has dependent academic records (Students, Course Offerings, Enrollments, Attendance, Exams, or Grades). Please deactivate the semester instead."
                });
            }

            _context.Semesters.Remove(semester);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Semester deleted successfully" });
        }
    }
}
