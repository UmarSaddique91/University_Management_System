using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.EnrollmentManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles =SystemRoles.superAdmin)]
    public class EnrollmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentController(ApplicationDbContext context)
        {
            _context = context;
        }



        //Put Api for Enrollment

        [HttpPost]
        public IActionResult AddEnrollment([FromBody] EnrollmentDTO model)
        {
            // Check whether the student is Exist or not
            var studentExists = _context.Students
                .Any(s => s.Id == model.StudentId);

            if (!studentExists)
            {
                return BadRequest(new { message = "Student does not exist" });
            }



            // Check Whether the Course Exist or Not
            var courseOfferingExists = _context.CourseOfferings
                .Any(co => co.Id == model.CourseOfferingId);

            if (!courseOfferingExists)
            {
                return BadRequest(new { message = "Course offering does not exist" });
            }



            // Check Duplicaiton
            var duplicateEnrollment = _context.Enrollments.Any(e =>
                e.StudentId == model.StudentId &&
                e.CourseOfferingId == model.CourseOfferingId);

            if (duplicateEnrollment)
            {
                return BadRequest(new { message = "Student is already enrolled in this course offering" });
            }

            

            var enrollment = new Enrollment
            {
                StudentId = model.StudentId,
                CourseOfferingId = model.CourseOfferingId,
                CreatedAt = model.CreatedAt,
                Status = model.Status
            };

            
            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Student enrolled successfully",
                enrollment.Id
            });
        }


        //GET API for Enrollment Controller

        [HttpGet]
        public IActionResult GetEnrollments()
        {
            
            var enrollments = _context.Enrollments
                .Select(e => new
                {
                    e.Id,

                    e.StudentId,
                    StudentName = e.Student.StudentName,

                    e.CourseOfferingId,
                    CourseName = e.CourseOffering.Course.CourseName,

                    TeacherName = e.CourseOffering.Teacher.TeacherName,
                    SemesterNumber = e.CourseOffering.Semester.SemesterNumber,

                    e.CreatedAt,
                    e.Status
                })
                .ToList();

            return Ok(enrollments);
        }

        //GET Specific Enrollment by id

        [HttpGet("{id}")]
        public IActionResult GetEnrollmentById(int id)
        {
            // Specific enrollment ko Id se find kar rahe hain
            var enrollment = _context.Enrollments
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,

                    e.StudentId,
                    StudentName = e.Student.StudentName,

                    e.CourseOfferingId,
                    CourseName = e.CourseOffering.Course.CourseName,

                    TeacherName = e.CourseOffering.Teacher.TeacherName,
                    SemesterNumber = e.CourseOffering.Semester.SemesterNumber,

                    e.CreatedAt,
                    e.Status
                })
                .FirstOrDefault();

            if (enrollment == null)
            {
                return NotFound(new { message = "Enrollment not found" });
            }

            return Ok(enrollment);
        }



        //Put API for Enrollement

        [HttpPut("{id}")]
        public IActionResult UpdateEnrollment(int id, [FromBody] EnrollmentDTO model)
        {
            // Check whether the Enrollment is Existing or not
            var enrollment = _context.Enrollments
                .FirstOrDefault(e => e.Id == id);

            if (enrollment == null)
            {
                return NotFound(new { message = "Enrollment not found" });
            }

            //Check whether the student is exisitng or not
            var studentExists = _context.Students
                .Any(s => s.Id == model.StudentId);

            if (!studentExists)
            {
                return BadRequest(new { message = "Student does not exist" });
            }



            // Check Whether the Course is existing or not
            var offeringExists = _context.CourseOfferings
                .Any(co => co.Id == model.CourseOfferingId);

            if (!offeringExists)
            {
                return BadRequest(new { message = "Course offering does not exist" });
            }

            //checked Duplicaiton

            var duplicateEnrollment = _context.Enrollments.Any(e =>
                e.StudentId == model.StudentId &&
                e.CourseOfferingId == model.CourseOfferingId &&
                e.Id != id);

            if (duplicateEnrollment)
            {
                return BadRequest(new
                {
                    message = "Student is already enrolled in this course offering"
                });
            }

            
            enrollment.StudentId = model.StudentId;
            enrollment.CourseOfferingId = model.CourseOfferingId;
            enrollment.CreatedAt = model.CreatedAt;
            enrollment.Status = model.Status;

            _context.SaveChanges();

            return Ok(new { message = "Enrollment updated successfully" });
        }


        //DELETE API

        [HttpDelete("{id}")]
        public IActionResult DeleteEnrollment(int id)
        {
            
            var enrollment = _context.Enrollments
                .FirstOrDefault(e => e.Id == id);

            if (enrollment == null)
            {
                return NotFound(new { message = "Enrollment not found" });
            }

            
            _context.Enrollments.Remove(enrollment);
            _context.SaveChanges();

            return Ok(new { message = "Enrollment deleted successfully" });
        }
    }
}
