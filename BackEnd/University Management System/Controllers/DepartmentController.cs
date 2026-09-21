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
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Department
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new
                {
                    d.Id,
                    d.DepartmentName,
                    ProgramCount = _context.AcademicPrograms.Count(p => p.DepartmentId == d.Id),
                    StudentCount = _context.Students.Count(s => s.DepartmentId == d.Id),
                    TeacherCount = _context.Teachers.Count(t => t.DepartmentId == d.Id),
                    CourseCount = _context.Courses.Count(c => c.DepartmentId == d.Id)
                })
                .ToListAsync();

            return Ok(departments);
        }

        // GET: api/Department/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await _context.Departments
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.DepartmentName,
                    ProgramCount = _context.AcademicPrograms.Count(p => p.DepartmentId == d.Id),
                    StudentCount = _context.Students.Count(s => s.DepartmentId == d.Id),
                    TeacherCount = _context.Teachers.Count(t => t.DepartmentId == d.Id),
                    CourseCount = _context.Courses.Count(c => c.DepartmentId == d.Id)
                })
                .FirstOrDefaultAsync();

            if (department == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            return Ok(department);
        }

        // POST: api/Department
        [HttpPost]
        [Authorize(Roles = SystemRoles.superAdmin)]
        public async Task<IActionResult> AddDepartment([FromBody] DepartmentDTO model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.DepartmentName))
            {
                return BadRequest(new { message = "Department name is required" });
            }

            var trimmedName = model.DepartmentName.Trim();

            var exists = await _context.Departments
                .AnyAsync(d => d.DepartmentName != null && d.DepartmentName.ToLower() == trimmedName.ToLower());

            if (exists)
            {
                return BadRequest(new { message = "Department name already exists" });
            }

            var department = new Department
            {
                DepartmentName = trimmedName
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Department added successfully",
                departmentId = department.Id
            });
        }

        // PUT: api/Department/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = SystemRoles.superAdmin)]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDTO model)
        {
            var existingDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
            if (existingDepartment == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            if (model == null || string.IsNullOrWhiteSpace(model.DepartmentName))
            {
                return BadRequest(new { message = "Department name is required" });
            }

            var trimmedName = model.DepartmentName.Trim();

            var duplicate = await _context.Departments
                .AnyAsync(d => d.DepartmentName != null && d.DepartmentName.ToLower() == trimmedName.ToLower() && d.Id != id);

            if (duplicate)
            {
                return BadRequest(new { message = "Department name already exists" });
            }

            existingDepartment.DepartmentName = trimmedName;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Department updated successfully" });
        }

        // DELETE: api/Department/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = SystemRoles.superAdmin)]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var departmentToDelete = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
            if (departmentToDelete == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            var hasStudents = await _context.Students.AnyAsync(s => s.DepartmentId == id);
            var hasTeachers = await _context.Teachers.AnyAsync(t => t.DepartmentId == id);
            var hasPrograms = await _context.AcademicPrograms.AnyAsync(p => p.DepartmentId == id);
            var hasCourses = await _context.Courses.AnyAsync(c => c.DepartmentId == id);

            if (hasStudents || hasTeachers || hasPrograms || hasCourses)
            {
                var relatedItems = new List<string>();
                if (hasStudents) relatedItems.Add("students");
                if (hasTeachers) relatedItems.Add("teachers");
                if (hasPrograms) relatedItems.Add("academic programs");
                if (hasCourses) relatedItems.Add("courses");

                return BadRequest(new
                {
                    message = $"Department cannot be deleted because related academic records exist ({string.Join(", ", relatedItems)})."
                });
            }

            try
            {
                _context.Departments.Remove(departmentToDelete);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Department deleted successfully" });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Department cannot be deleted because related records exist." });
            }
        }
    }
}
