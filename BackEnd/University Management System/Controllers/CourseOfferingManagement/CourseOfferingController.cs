using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.CourseOfferingManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin Access")]
    public class CourseOfferingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.EnrollmentSyncService _enrollmentSyncService;

        public CourseOfferingController(ApplicationDbContext context, Services.EnrollmentSyncService enrollmentSyncService)
        {
            _context = context;
            _enrollmentSyncService = enrollmentSyncService;
        }

        // GET: api/CourseOffering
        [HttpGet]
        public async Task<IActionResult> GetCourseOfferings()
        {
            var offerings = await _context.CourseOfferings
                .Select(co => new
                {
                    co.Id,
                    co.CourseId,
                    CourseCode = co.Course != null ? co.Course.CourseCode : "",
                    CourseName = co.Course != null ? co.Course.CourseName : "",
                    CreditHours = co.Course != null ? (int?)co.Course.CreditHours : null,

                    co.TeacherId,
                    TeacherName = co.Teacher != null ? co.Teacher.TeacherName : "",
                    TeacherEmail = co.Teacher != null ? co.Teacher.Email : "",

                    DepartmentId = co.Course != null ? (int?)co.Course.DepartmentId : (co.Teacher != null ? (int?)co.Teacher.DepartmentId : null),
                    DepartmentName = co.Course != null && co.Course.Department != null
                        ? co.Course.Department.DepartmentName
                        : (co.Teacher != null && co.Teacher.Department != null ? co.Teacher.Department.DepartmentName : ""),

                    co.SemesterId,
                    SemesterNumber = co.Semester != null ? co.Semester.SemesterNumber : 0,

                    AcademicProgramId = co.Semester != null ? co.Semester.AcademicProgramId : 0,
                    ProgramName = co.Semester != null && co.Semester.AcademicProgram != null ? co.Semester.AcademicProgram.ProgramName : "",
                    ProgramCode = co.Semester != null && co.Semester.AcademicProgram != null ? co.Semester.AcademicProgram.ProgramCode : "",

                    AcademicSessionId = co.Semester != null ? co.Semester.AcademicSessionId : 0,
                    SessionName = co.Semester != null && co.Semester.AcademicSession != null ? co.Semester.AcademicSession.SessionName : "",

                    co.Status
                })
                .OrderByDescending(co => co.Id)
                .ToListAsync();

            return Ok(offerings);
        }

        // POST: api/CourseOffering
        [HttpPost]
        public async Task<IActionResult> AddCourseOffering([FromBody] CourseAssignmentDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.SemesterId <= 0)
            {
                return BadRequest(new { message = "Semester must be selected" });
            }

            if (model.CourseId <= 0)
            {
                return BadRequest(new { message = "Course must be selected" });
            }

            if (model.TeacherId <= 0)
            {
                return BadRequest(new { message = "Teacher must be selected" });
            }

            // 1. Validate Semester
            var semester = await _context.Semesters
                .Include(s => s.AcademicProgram)
                .FirstOrDefaultAsync(s => s.Id == model.SemesterId);

            if (semester == null)
            {
                return BadRequest(new { message = "Selected semester does not exist" });
            }

            if (semester.Status != null && semester.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Cannot assign a course offering to an inactive semester." });
            }

            // 2. Validate Course
            var course = await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            if (course == null)
            {
                return BadRequest(new { message = "Selected course does not exist" });
            }

            if (course.Status != null && course.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Cannot assign an inactive course" });
            }

            // 3. Validate Course is mapped to Semester's Academic Program
            var isMapped = await _context.AcademicProgramCourses
                .AnyAsync(apc => apc.AcademicProgramId == semester.AcademicProgramId && apc.CourseId == course.Id);

            if (!isMapped)
            {
                return BadRequest(new
                {
                    message = $"Course '{course.CourseName}' is not mapped to the program '{semester.AcademicProgram?.ProgramName ?? "Selected Program"}'"
                });
            }

            // 4. Validate Teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == model.TeacherId);

            if (teacher == null)
            {
                return BadRequest(new { message = "Selected teacher does not exist" });
            }

            if (teacher.Status != null && teacher.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Cannot assign an inactive teacher" });
            }

            if (teacher.DepartmentId.HasValue && course.DepartmentId.HasValue && teacher.DepartmentId.Value != course.DepartmentId.Value)
            {
                return BadRequest(new { message = "Teacher does not belong to the department of the course being offered" });
            }

            // 5. Check Uniqueness: CourseId + SemesterId
            var duplicateOffering = await _context.CourseOfferings
                .AnyAsync(co => co.CourseId == model.CourseId && co.SemesterId == model.SemesterId);

            if (duplicateOffering)
            {
                return BadRequest(new
                {
                    message = $"This course is already offered in Semester {semester.SemesterNumber}. Duplicate course offerings in the same semester are not allowed."
                });
            }

            var offering = new CourseOffering
            {
                CourseId = model.CourseId,
                TeacherId = model.TeacherId,
                SemesterId = model.SemesterId,
                Status = string.IsNullOrWhiteSpace(model.Status) ? "Active" : model.Status.Trim()
            };

            _context.CourseOfferings.Add(offering);
            await _context.SaveChangesAsync();

            if (offering.Status == "Active")
            {
                await _enrollmentSyncService.SynchronizeSemesterEnrollmentsAsync(offering.SemesterId);
            }

            return Ok(new
            {
                message = "Course assignment created successfully",
                offering.Id
            });
        }

        // PUT: api/CourseOffering/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourseOffering(int id, [FromBody] CourseAssignmentDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var offering = await _context.CourseOfferings.FirstOrDefaultAsync(co => co.Id == id);
            if (offering == null)
            {
                return NotFound(new { message = "Course offering not found" });
            }

            // 1. Validate Semester
            var semester = await _context.Semesters
                .Include(s => s.AcademicProgram)
                .FirstOrDefaultAsync(s => s.Id == model.SemesterId);

            if (semester == null)
            {
                return BadRequest(new { message = "Selected semester does not exist" });
            }

            if (semester.Status != null && semester.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Cannot assign a course offering to an inactive semester." });
            }

            // 2. Validate Course
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            if (course == null)
            {
                return BadRequest(new { message = "Selected course does not exist" });
            }

            if (course.Status != null && course.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Cannot assign an inactive course" });
            }

            // 3. Validate Course is mapped to Semester's Academic Program
            var isMapped = await _context.AcademicProgramCourses
                .AnyAsync(apc => apc.AcademicProgramId == semester.AcademicProgramId && apc.CourseId == course.Id);

            if (!isMapped)
            {
                return BadRequest(new
                {
                    message = $"Course '{course.CourseName}' is not mapped to the program '{semester.AcademicProgram?.ProgramName ?? "Selected Program"}'"
                });
            }

            // 4. Validate Teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == model.TeacherId);

            if (teacher == null)
            {
                return BadRequest(new { message = "Selected teacher does not exist" });
            }

            if (teacher.Status != null && teacher.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Cannot assign an inactive teacher" });
            }

            if (teacher.DepartmentId.HasValue && course.DepartmentId.HasValue && teacher.DepartmentId.Value != course.DepartmentId.Value)
            {
                return BadRequest(new { message = "Teacher does not belong to the department of the course being offered" });
            }

            // 5. Check Uniqueness: CourseId + SemesterId (excluding current record)
            var duplicateOffering = await _context.CourseOfferings
                .AnyAsync(co => co.CourseId == model.CourseId && co.SemesterId == model.SemesterId && co.Id != id);

            if (duplicateOffering)
            {
                return BadRequest(new
                {
                    message = $"This course is already offered in Semester {semester.SemesterNumber}. Duplicate course offerings in the same semester are not allowed."
                });
            }

            offering.CourseId = model.CourseId;
            offering.TeacherId = model.TeacherId;
            offering.SemesterId = model.SemesterId;
            if (!string.IsNullOrWhiteSpace(model.Status))
            {
                offering.Status = model.Status.Trim();
            }

            await _context.SaveChangesAsync();

            if (offering.Status == "Active")
            {
                await _enrollmentSyncService.SynchronizeSemesterEnrollmentsAsync(offering.SemesterId);
            }

            return Ok(new { message = "Course assignment updated successfully" });
        }

        // PATCH: api/CourseOffering/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] StatusUpdateDTO model)
        {
            var offering = await _context.CourseOfferings.FirstOrDefaultAsync(co => co.Id == id);
            if (offering == null)
            {
                return NotFound(new { message = "Course offering not found" });
            }

            var nextStatus = !string.IsNullOrWhiteSpace(model?.Status)
                ? model.Status.Trim()
                : (offering.Status == "Active" ? "Inactive" : "Active");

            offering.Status = nextStatus;
            await _context.SaveChangesAsync();

            if (offering.Status == "Active")
            {
                await _enrollmentSyncService.SynchronizeSemesterEnrollmentsAsync(offering.SemesterId);
            }

            return Ok(new { message = $"Status updated to {nextStatus}", status = nextStatus });
        }

        // DELETE: api/CourseOffering/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseOffering(int id)
        {
            var offering = await _context.CourseOfferings.FirstOrDefaultAsync(co => co.Id == id);
            if (offering == null)
            {
                return NotFound(new { message = "Course offering not found" });
            }

            // Safe Deletion Checks: Verify if referenced by Enrollments, Attendance, or Exams
            var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.CourseOfferingId == id);
            var hasAttendance = await _context.Attendance.AnyAsync(a => a.CourseOfferingId == id);
            var hasExams = await _context.Exams.AnyAsync(ex => ex.CourseOfferingId == id);

            if (hasEnrollments || hasAttendance || hasExams)
            {
                return BadRequest(new
                {
                    message = "Cannot delete course assignment because it is already referenced by student enrollments, attendance records, or examinations. Deactivate the assignment instead."
                });
            }

            _context.CourseOfferings.Remove(offering);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Course assignment deleted successfully" });
        }

        // GET: api/CourseOffering/workload
        [HttpGet("workload")]
        public async Task<IActionResult> GetTeacherWorkload()
        {
            var workload = await _context.Teachers
                .Select(t => new
                {
                    TeacherId = t.Id,
                    TeacherCode = t.TeacherId,
                    TeacherName = t.TeacherName,
                    TeacherEmail = t.Email,
                    DepartmentId = t.DepartmentId,
                    DepartmentName = t.Department != null ? t.Department.DepartmentName : "",
                    DistinctCoursesCount = _context.CourseOfferings
                        .Where(co => co.TeacherId == t.Id && co.Status == "Active")
                        .Select(co => co.CourseId)
                        .Distinct()
                        .Count(),
                    ActiveOfferingsCount = _context.CourseOfferings
                        .Count(co => co.TeacherId == t.Id && co.Status == "Active"),
                    TotalOfferingsCount = _context.CourseOfferings
                        .Count(co => co.TeacherId == t.Id)
                })
                .OrderByDescending(w => w.ActiveOfferingsCount)
                .ToListAsync();

            return Ok(workload);
        }
    }
}
