using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssignmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        //POST API
        [HttpPost]
        public IActionResult AddAssignment([FromBody] Assignment assignment)
        {
            if (assignment == null)
                return BadRequest("Invalid data");

            _context.Assignment.Add(assignment);
            _context.SaveChanges();
            return Ok("Assignment added successfully");
        }

        //GET API
        [HttpGet]
        
        public IActionResult GetAllAssignments(string?search, string?semester, string?status)
        {
            var query = _context.Assignment
       .Include(a => a.Course)
       .Include(a => a.AssignedTeacher)
       .AsQueryable();

            //for Searching Filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(tr => tr.AssignmentCode.Contains(search) ||
                tr.Course.CourseName.Contains(search) ||
                tr.AssignedTeacher.TeacherName.Contains(search) ||
                tr.Department.Contains(search));
            }

            //for Semester Filter

            if (!string.IsNullOrEmpty(semester))
            {
                query = query.Where(tr => tr.Semester == semester);
            }

            //for Status Filter

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(tr => tr.Status == status);
            }

            var assignments = query.ToList();

            return Ok(assignments);
        }

        // PUT API
        [HttpPut("{id}")]
        public IActionResult UpdateAssignment(int id, [FromBody] Assignment updated)
        {
            var existing = _context.Assignment.Find(id);

            if (existing == null)
                return NotFound("Assignment not found");

            if (!string.IsNullOrEmpty(updated.AssignmentCode))
            existing.AssignmentCode = updated.AssignmentCode;
            if (!string.IsNullOrEmpty(updated.Department))
                existing.Department = updated.Department;
            if (updated.Credits>0)
                existing.Credits = updated.Credits;
            if (!string.IsNullOrEmpty(updated.Semester))
                existing.Semester = updated.Semester;
            if (updated.Capacity.HasValue)
                existing.Capacity = updated.Capacity;
            if (updated.CourseId>0)
                existing.CourseId = updated.CourseId;
            if (updated.AssignedTeacherId>0)
                existing.AssignedTeacherId = updated.AssignedTeacherId;

            _context.SaveChanges();
            return Ok("Assignment updated successfully");
        }

        // DELETE API
        [HttpDelete("{id}")]
        public IActionResult DeleteAssignment(int id)
        {
            var assignment = _context.Assignment.Find(id);

            if (assignment == null)
                return NotFound("Assignment not found");

            _context.Assignment.Remove(assignment);
            _context.SaveChanges();
            return Ok("Assignment deleted successfully");
        }
    }
}
