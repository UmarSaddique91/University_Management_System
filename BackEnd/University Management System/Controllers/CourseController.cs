using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin Access")]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Course
        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _context.Courses
                .Select(c => new
                {
                    c.Id,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours,
                    c.Level,
                    c.DepartmentId,
                    DepartmentName = c.Department != null ? c.Department.DepartmentName : null,
                    Status = c.Status ?? "Active",
                    AcademicPrograms = _context.AcademicProgramCourses
                        .Where(apc => apc.CourseId == c.Id)
                        .Select(apc => new
                        {
                            Id = apc.AcademicProgramId,
                            ProgramName = apc.AcademicProgram != null ? apc.AcademicProgram.ProgramName : null,
                            ProgramCode = apc.AcademicProgram != null ? apc.AcademicProgram.ProgramCode : null
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(courses);
        }

        // GET: api/Course/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _context.Courses
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours,
                    c.Level,
                    c.DepartmentId,
                    DepartmentName = c.Department != null ? c.Department.DepartmentName : null,
                    Status = c.Status ?? "Active",
                    AcademicPrograms = _context.AcademicProgramCourses
                        .Where(apc => apc.CourseId == c.Id)
                        .Select(apc => new
                        {
                            Id = apc.AcademicProgramId,
                            ProgramName = apc.AcademicProgram != null ? apc.AcademicProgram.ProgramName : null,
                            ProgramCode = apc.AcademicProgram != null ? apc.AcademicProgram.ProgramCode : null
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound(new { message = "Course not found" });
            }

            return Ok(course);
        }

        // POST: api/Course
        [HttpPost]
        [Authorize(Roles = SystemRoles.superAdmin)]
        public async Task<IActionResult> AddCourse([FromBody] CourseDTO model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Course data is invalid" });
            }

            if (string.IsNullOrWhiteSpace(model.CourseCode))
            {
                return BadRequest(new { message = "Course code is required" });
            }

            if (string.IsNullOrWhiteSpace(model.CourseName))
            {
                return BadRequest(new { message = "Course name is required" });
            }

            if (!model.CreditHours.HasValue || model.CreditHours <= 0)
            {
                return BadRequest(new { message = "Credit hours must be a positive number" });
            }

            if (string.IsNullOrWhiteSpace(model.Level))
            {
                return BadRequest(new { message = "Level is required" });
            }

            if (!model.DepartmentId.HasValue)
            {
                return BadRequest(new { message = "Department is required" });
            }

            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == model.DepartmentId.Value);

            if (!departmentExists)
            {
                return BadRequest(new { message = "Department does not exist" });
            }

            var trimmedCode = model.CourseCode.Trim();
            var duplicate = await _context.Courses
                .AnyAsync(c => c.CourseCode != null && c.CourseCode.ToLower() == trimmedCode.ToLower());

            if (duplicate)
            {
                return BadRequest(new { message = "Course code already exists" });
            }

            var status = string.IsNullOrWhiteSpace(model.Status) ? "Active" : model.Status.Trim();
            if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                status = "Active";
            }
            else
            {
                status = status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? "Active" : "Inactive";
            }

            var course = new Course
            {
                CourseCode = trimmedCode,
                CourseName = model.CourseName.Trim(),
                CreditHours = model.CreditHours.Value,
                Level = model.Level.Trim(),
                DepartmentId = model.DepartmentId.Value,
                Status = status
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Create AcademicProgramCourses mappings if provided
            if (model.AcademicProgramIds != null && model.AcademicProgramIds.Any())
            {
                var distinctProgramIds = model.AcademicProgramIds.Distinct().ToList();
                var validProgramIds = await _context.AcademicPrograms
                    .Where(p => distinctProgramIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                foreach (var progId in validProgramIds)
                {
                    _context.AcademicProgramCourses.Add(new AcademicProgramCourse
                    {
                        AcademicProgramId = progId,
                        CourseId = course.Id
                    });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Course added successfully",
                course.Id
            });
        }

        // PUT: api/Course/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = SystemRoles.superAdmin)]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseDTO model)
        {
            var existingCourse = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (existingCourse == null)
            {
                return NotFound(new { message = "Course not found" });
            }

            if (model == null)
            {
                return BadRequest(new { message = "Course data is invalid" });
            }

            if (string.IsNullOrWhiteSpace(model.CourseCode))
            {
                return BadRequest(new { message = "Course code is required" });
            }

            if (string.IsNullOrWhiteSpace(model.CourseName))
            {
                return BadRequest(new { message = "Course name is required" });
            }

            if (!model.CreditHours.HasValue || model.CreditHours <= 0)
            {
                return BadRequest(new { message = "Credit hours must be a positive number" });
            }

            if (string.IsNullOrWhiteSpace(model.Level))
            {
                return BadRequest(new { message = "Level is required" });
            }

            if (!model.DepartmentId.HasValue)
            {
                return BadRequest(new { message = "Department is required" });
            }

            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == model.DepartmentId.Value);

            if (!departmentExists)
            {
                return BadRequest(new { message = "Department does not exist" });
            }

            var trimmedCode = model.CourseCode.Trim();
            var duplicate = await _context.Courses
                .AnyAsync(c => c.CourseCode != null && c.CourseCode.ToLower() == trimmedCode.ToLower() && c.Id != id);

            if (duplicate)
            {
                return BadRequest(new { message = "Course code already exists" });
            }

            existingCourse.CourseCode = trimmedCode;
            existingCourse.CourseName = model.CourseName.Trim();
            existingCourse.CreditHours = model.CreditHours.Value;
            existingCourse.Level = model.Level.Trim();
            existingCourse.DepartmentId = model.DepartmentId.Value;

            if (!string.IsNullOrWhiteSpace(model.Status))
            {
                var status = model.Status.Trim();
                if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    existingCourse.Status = "Active";
                else if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    existingCourse.Status = "Inactive";
            }

            // Synchronize AcademicProgramCourses mappings if provided
            if (model.AcademicProgramIds != null)
            {
                var distinctProgramIds = model.AcademicProgramIds.Distinct().ToList();
                var validProgramIds = await _context.AcademicPrograms
                    .Where(p => distinctProgramIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var existingMappings = await _context.AcademicProgramCourses
                    .Where(apc => apc.CourseId == id)
                    .ToListAsync();

                // Remove mappings no longer selected
                var toRemove = existingMappings
                    .Where(em => !validProgramIds.Contains(em.AcademicProgramId))
                    .ToList();
                _context.AcademicProgramCourses.RemoveRange(toRemove);

                // Add newly selected mappings
                var existingProgIds = existingMappings.Select(em => em.AcademicProgramId).ToHashSet();
                var toAdd = validProgramIds
                    .Where(pid => !existingProgIds.Contains(pid))
                    .ToList();

                foreach (var progId in toAdd)
                {
                    _context.AcademicProgramCourses.Add(new AcademicProgramCourse
                    {
                        AcademicProgramId = progId,
                        CourseId = id
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Course updated successfully" });
        }

        // PATCH: api/Course/{id}/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = SystemRoles.superAdmin)]
        public async Task<IActionResult> UpdateCourseStatus(int id, [FromBody] UpdateCourseStatusDTO model)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                return NotFound(new { message = "Course not found" });
            }

            if (model == null || string.IsNullOrWhiteSpace(model.Status))
            {
                return BadRequest(new { message = "Status is required" });
            }

            var trimmedStatus = model.Status.Trim();
            if (!trimmedStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                !trimmedStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Status must be either 'Active' or 'Inactive'" });
            }

            var newStatus = trimmedStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) ? "Active" : "Inactive";
            course.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Course status updated to {newStatus}",
                status = newStatus
            });
        }

        // DELETE: api/Course/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = SystemRoles.superAdmin)]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var courseToDelete = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (courseToDelete == null)
            {
                return NotFound(new { message = "Course not found" });
            }

            // Safe Delete: Check if referenced by active course offerings (which carry enrollments/grades)
            var isUsedInOfferings = await _context.CourseOfferings
                .AnyAsync(co => co.CourseId == id);

            if (isUsedInOfferings)
            {
                return BadRequest(new
                {
                    message = "Course cannot be deleted because it is already used in academic course offerings."
                });
            }

            // Clean up any curriculum program mappings
            var programMappings = await _context.AcademicProgramCourses
                .Where(apc => apc.CourseId == id)
                .ToListAsync();

            if (programMappings.Any())
            {
                _context.AcademicProgramCourses.RemoveRange(programMappings);
            }

            try
            {
                _context.Courses.Remove(courseToDelete);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Course deleted successfully" });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Course cannot be deleted because related records exist." });
            }
        }
    }
}
