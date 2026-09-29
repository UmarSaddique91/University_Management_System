using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models;

namespace University_Management_System.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]

    [Authorize(Policy = "Admin Access")]
    public class TeacherController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TeacherController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/Teacher
        [HttpPost]
        public async Task<IActionResult> AddTeacher([FromBody] AddTeacherDTO teacher)
        {
            if (teacher == null)
            {
                return BadRequest(new { message = "Teacher data is invalid" });
            }

            if (string.IsNullOrWhiteSpace(teacher.TeacherName))
            {
                return BadRequest(new { message = "Teacher name is required" });
            }

            if (string.IsNullOrWhiteSpace(teacher.TeacherId))
            {
                return BadRequest(new { message = "Teacher ID is required" });
            }

            if (string.IsNullOrWhiteSpace(teacher.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            if (!teacher.DepartmentId.HasValue)
            {
                return BadRequest(new { message = "Department is required" });
            }

            // Check duplication of TeacherId
            var existingTeacherId = await _context.Teachers.AnyAsync(t => t.TeacherId == teacher.TeacherId);
            if (existingTeacherId)
            {
                return BadRequest(new { message = "Teacher ID already exists" });
            }

            // Check duplication of Email
            var normalizedEmail = teacher.Email.Trim().ToLower();
            var existingTeacherEmail = await _context.Teachers.AnyAsync(t => t.Email != null && t.Email.ToLower() == normalizedEmail);
            if (existingTeacherEmail)
            {
                return BadRequest(new { message = "A teacher with this email already exists" });
            }

            var existingUser = await _userManager.FindByEmailAsync(teacher.Email.Trim());
            if (existingUser != null)
            {
                return BadRequest(new { message = "A user account with this email already exists" });
            }

            // Check whether Department exists
            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == teacher.DepartmentId.Value);
            if (!departmentExists)
            {
                return BadRequest(new { message = "Department not found" });
            }

            var newTeacher = new Teacher
            {
                TeacherName = teacher.TeacherName.Trim(),
                TeacherId = teacher.TeacherId.Trim(),
                DepartmentId = teacher.DepartmentId,
                Specialization = teacher.Specialization?.Trim(),
                Email = teacher.Email.Trim(),
                UserId = null,
                Status = "Inactive",
                CreatedAt = DateTime.UtcNow
            };

            _context.Teachers.Add(newTeacher);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Teacher Added Successfully", teacherId = newTeacher.Id });
        }

        // GET: api/Teacher
        
        [HttpGet]
        public async Task<IActionResult> GetTeachers(string? name, string? teacherId, int? DepartmentId, string? status)
        {
            var query = _context.Teachers.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(tr => tr.TeacherName != null && tr.TeacherName.Contains(name));

            if (!string.IsNullOrEmpty(teacherId))
                query = query.Where(tr => tr.TeacherId != null && tr.TeacherId.Contains(teacherId));

            if (DepartmentId.HasValue)
                query = query.Where(tr => tr.DepartmentId == DepartmentId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(tr => tr.Status != null && tr.Status.Contains(status));

            var result = await query.Select(t => new AddTeacherDTO
            {
                Id = t.Id,
                TeacherId = t.TeacherId,
                UserId = t.UserId,
                Email = t.Email,
                TeacherName = t.TeacherName,
                DepartmentId = t.DepartmentId,
                DepartmentName = t.Department != null ? t.Department.DepartmentName : null,
                Specialization = t.Specialization,
                Status = t.Status,
                AssignedCoursesCount = _context.CourseOfferings
                    .Where(co => co.TeacherId == t.Id && co.Status == "Active")
                    .Select(co => co.CourseId)
                    .Distinct()
                    .Count()
            }).ToListAsync();

            return Ok(result);
        }

        // PUT: api/Teacher/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] AddTeacherDTO teacher)
        {
            var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id);
            if (existingTeacher == null)
            {
                return NotFound(new { message = "Teacher Not Found" });
            }

            if (string.IsNullOrWhiteSpace(teacher.TeacherName))
            {
                return BadRequest(new { message = "Teacher name is required" });
            }

            if (string.IsNullOrWhiteSpace(teacher.TeacherId))
            {
                return BadRequest(new { message = "Teacher ID is required" });
            }

            // Check duplicate TeacherId
            var duplicateId = await _context.Teachers.AnyAsync(t => t.TeacherId == teacher.TeacherId && t.Id != id);
            if (duplicateId)
            {
                return BadRequest(new { message = "Teacher ID already exists" });
            }

            // Check department
            if (teacher.DepartmentId.HasValue)
            {
                var depExists = await _context.Departments.AnyAsync(d => d.Id == teacher.DepartmentId.Value);
                if (!depExists)
                {
                    return BadRequest(new { message = "Department does not exist" });
                }
            }

            existingTeacher.TeacherName = teacher.TeacherName.Trim();
            existingTeacher.TeacherId = teacher.TeacherId.Trim();
            existingTeacher.DepartmentId = teacher.DepartmentId;
            existingTeacher.Specialization = teacher.Specialization?.Trim();

            await _context.SaveChangesAsync();
            return Ok(new { message = "Teacher Updated Successfully" });
        }

        // PATCH: api/Teacher/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTeacherStatus(int id, [FromBody] UpdateTeacherStatusDTO model)
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

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            if (newStatus == "Active" && (string.IsNullOrEmpty(teacher.UserId) || string.IsNullOrEmpty(teacher.Email)))
            {
                return BadRequest(new { message = "Cannot activate teacher who has not completed registration." });
            }

            teacher.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Teacher status updated to {newStatus} successfully", status = newStatus });
        }

        // DELETE: api/Teacher/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacherToDelete = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id);
            if (teacherToDelete == null)
            {
                return NotFound(new { message = "Teacher Not Found" });
            }

            if (!string.IsNullOrEmpty(teacherToDelete.UserId))
            {
                var user = await _userManager.FindByIdAsync(teacherToDelete.UserId);
                if (user != null)
                {
                    var userResult = await _userManager.DeleteAsync(user);
                    if (!userResult.Succeeded)
                    {
                        return BadRequest(userResult.Errors);
                    }
                }
            }

            _context.Teachers.Remove(teacherToDelete);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Teacher deleted successfully" });
        }
    }
}

