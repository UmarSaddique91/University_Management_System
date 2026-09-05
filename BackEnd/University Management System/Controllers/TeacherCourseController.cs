using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;
using Microsoft.EntityFrameworkCore;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherCourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeacherCourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        //PostApi

        [HttpPost]
        public IActionResult AssignCourse([FromBody] TeacherCourse teacherCourse)
        {
            if (teacherCourse == null)
            {
                return BadRequest("Invalid data");
            }
                
            _context.TeacherCourse.Add(teacherCourse);
            _context.SaveChanges();
            return Ok("Course assigned successfully");
        }


        //GetAPI

        [HttpGet]
        public IActionResult GetAllAssignments()
        {
            var assignments = _context.TeacherCourse
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .ToList();

            return Ok(assignments);
        }

        //PutAPI

        [HttpPut("{teacherId}/{courseId}")]
        public IActionResult UpdateAssignment(int teacherId, int courseId, [FromBody] TeacherCourse updated)
        {
            var existing = _context.TeacherCourse
                .FirstOrDefault(tc => tc.TeacherId == teacherId && tc.CourseId == courseId);

            if (existing == null)
                return NotFound("Assignment not found");

            existing.TeacherId = updated.TeacherId;
            existing.CourseId = updated.CourseId;

            _context.SaveChanges();
            return Ok("Assignment updated successfully");
        }

        //DeleteAPI

        [HttpDelete("{teacherId}/{courseId}")]
        public IActionResult DeleteAssignment(int teacherId, int courseId)
        {
            var assignment = _context.TeacherCourse
                .FirstOrDefault(tc => tc.TeacherId == teacherId && tc.CourseId == courseId);

            if (assignment == null)
                return NotFound("Assignment not found");

            _context.TeacherCourse.Remove(assignment);
            _context.SaveChanges();
            return Ok("Assignment deleted successfully");
        }
    }
}
