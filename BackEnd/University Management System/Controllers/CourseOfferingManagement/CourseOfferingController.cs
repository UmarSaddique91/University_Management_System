using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.CourseOfferingManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles = SystemRoles.superAdmin)]
    public class CourseOfferingController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public CourseOfferingController (ApplicationDbContext context)
        {
            _context = context;
        }

        //Post API for CourseOfferting
        [HttpPost]
        public IActionResult AddCourseOffering([FromBody] CourseOfferingDTO model)
        {

            var offering = new CourseOffering
            {
                CourseId = model.CourseId,
                TeacherId = model.TeacherId,
                SemesterId = model.SemesterId
            };


            //Check whether the course Exist or not
            var courseExists = _context.Courses
                .Any(c => c.Id == model.CourseId);

            if (!courseExists)
            {
                return BadRequest(new { message = "Course does not exist" });
            }

            //Check whether the teacher exist or not
            var teacherExists = _context.Teachers
                .Any(t => t.Id == model.TeacherId);

            if (!teacherExists)
            {
                return BadRequest(new { message = "Teacher does not exist" });
            }

            // Check whether the semester Exist or not
            var semesterExists = _context.Semesters
                .Any(s => s.Id == model.SemesterId);

            if (!semesterExists)
            {
                return BadRequest(new { message = "Semester does not exist" });
            }

            

            //Check Duplicaiton
            var duplicateOffering = _context.CourseOfferings.Any(co =>
                co.CourseId == model.CourseId &&
                co.TeacherId == model.TeacherId &&
                co.SemesterId == model.SemesterId);

            if (duplicateOffering)
            {
                return BadRequest(new { message = "Course offering already exists" });
            }

            
            
            _context.CourseOfferings.Add(offering);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Course offering added successfully",
                offering.Id
            });
        }



        //Get Api for Course Offering

        [HttpGet]
        public IActionResult GetCourseOfferings()
        {
            // IDs ke sath readable names bhi return kar rahe hain
            var offerings = _context.CourseOfferings
                .Select(co => new
                {
                    co.Id,

                    co.CourseId,
                    CourseName = co.Course.CourseName,

                    co.TeacherId,
                    TeacherName = co.Teacher.TeacherName,

                    co.SemesterId,
                    SemesterNumber = co.Semester.SemesterNumber
                })
                .ToList();

            return Ok(offerings);
        }


        //Put API for Course Offering

        [HttpPut("{id}")]
        public IActionResult UpdateCourseOffering(
    int id,
    [FromBody] CourseOfferingDTO model)
        {
            // Existing offering find kr rhe hain
            var offering = _context.CourseOfferings
                .FirstOrDefault(co => co.Id == id);

            if (offering == null)
            {
                return NotFound(new { message = "Course offering not found" });
            }

            // Check whether Course Exist or not

            var courseExists = _context.Courses
                .Any(c => c.Id == model.CourseId);

            if (!courseExists)
            {
                return BadRequest(new { message = "Course does not exist" });
            }

            // Check whether Teacher Exist or not

            var teacherExists = _context.Teachers
                .Any(t => t.Id == model.TeacherId);

            if (!teacherExists)
            {
                return BadRequest(new { message = "Teacher does not exist" });
            }

            // Check whether Semester Exist or not

            var semesterExists = _context.Semesters
                .Any(s => s.Id == model.SemesterId);

            if (!semesterExists)
            {
                return BadRequest(new { message = "Semester does not exist" });
            }

            // Cehck Duplocation
            var duplicateOffering = _context.CourseOfferings.Any(co =>
                co.CourseId == model.CourseId &&
                co.TeacherId == model.TeacherId &&
                co.SemesterId == model.SemesterId &&
                co.Id != id);

            if (duplicateOffering)
            {
                return BadRequest(new { message = "Course offering already exists" });
            }

            // Existing record update kar rahe hain
            offering.CourseId = model.CourseId;
            offering.TeacherId = model.TeacherId;
            offering.SemesterId = model.SemesterId;

            _context.SaveChanges();

            return Ok(new { message = "Course offering updated successfully" });
        }




        //DeleteApi for CourseOffering

        [HttpDelete("{id}")]
        public IActionResult DeleteCourseOffering(int id)
        {
            
            var offering = _context.CourseOfferings
                .FirstOrDefault(co => co.Id == id);

            if (offering == null)
            {
                return NotFound(new { message = "Course offering not found" });
            }

            
            _context.CourseOfferings.Remove(offering);
            _context.SaveChanges();

            return Ok(new { message = "Course offering deleted successfully" });
        }
    }
}
