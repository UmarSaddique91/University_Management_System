using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Post Api

        [HttpPost]

        public IActionResult AddCourse([FromBody] Course course)
        {
            if (course == null)
            {
                return BadRequest("Course Data is Invalid");
            }

            _context.Courses.Add(course);
            _context.SaveChanges();
            return Ok("Course Added Successfully");
        }

        //Get Api

        public IActionResult GetCourses(string? CourseCode, string?CourseName, string?DepartmentName, int?CreditHours, string?Level, string?Status)
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(CourseCode))
                query = query.Where(tr => tr.CourseCode.Contains(CourseCode));

            if (!string.IsNullOrEmpty(CourseName))
                query = query.Where(tr => tr.CourseName.Contains(CourseName));

            if (!string.IsNullOrEmpty(DepartmentName))
                query = query.Where(tr => tr.DepartmentName.Contains(DepartmentName));

            if (CreditHours>0)
                query = query.Where(tr => tr.CreditHours==CreditHours);

            if (!string.IsNullOrEmpty(Level))
                query = query.Where(tr => tr.Level.Contains(Level));

            if (!string.IsNullOrEmpty(Status))
                query = query.Where(tr => tr.Status.Contains(Status));

            var result = query.ToList();
            return Ok(result);
        }

        //Update Api

        [HttpPut ("{Id}")]

        public IActionResult UpdateCourse(int Id, [FromBody] Course course)
        {
            var ExistingCourse = _context.Courses.Find(Id);
            if (ExistingCourse == null)
            {
                return NotFound("Course Not Found");
            }

            if(!string.IsNullOrEmpty(course.CourseCode))
            ExistingCourse.CourseCode = course.CourseCode;

            if (!string.IsNullOrEmpty(course.CourseName))
                ExistingCourse.CourseName = course.CourseName;

            if (!string.IsNullOrEmpty(course.DepartmentName))
                ExistingCourse.DepartmentName = course.DepartmentName;

            if (course.CreditHours>0)
                ExistingCourse.CreditHours = course.CreditHours;

            if (!string.IsNullOrEmpty(course.Status))
                ExistingCourse.Status = course.Status;

            if (!string.IsNullOrEmpty(course.Level))
                ExistingCourse.Level = course.Level;

            _context.SaveChanges();
            return Ok("Course Updated Successfully");
        }

        //DeleteApi

        [HttpDelete("{Id}")]

        public IActionResult DeleteCourse(int Id, [FromBody] Course course)
        {
            var CourseToDelete = _context.Courses.Find(Id);

            if (CourseToDelete == null)
            {
                return BadRequest("Course Not Found");
            }

            _context.Courses.Remove(CourseToDelete);
            _context.SaveChanges();
            return Ok("Course Deleted Successfully");
        }
    }
}
