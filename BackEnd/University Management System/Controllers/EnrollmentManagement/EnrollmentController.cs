using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.EnrollmentManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin Access")]
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


        //Get Api for Enrollment Graph Trend

        [HttpGet("trend")]

        public async Task<IActionResult> GetEnrollmentTrend([FromQuery] string period="6 Months")
        {

            var now = DateTime.UtcNow;

            

            var startOfMonth = new DateTime(

                now.Year,
                now.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc
                );

            var startDate = period switch
            {
                "1 Year" => startOfMonth.AddMonths(-11),
                "6 Months" => startOfMonth.AddMonths(-5),
                "3 Months" => startOfMonth.AddMonths(-2),
                "1 Month" => startOfMonth,
                "1 Week" => now.AddDays(-7),
                _=> now.AddMonths(-5)
            };


            var activeEnrollment =await  _context.Enrollments.Where(
                en => en.Status == "active" &&
                en.CreatedAt >= startDate &&
               en.CreatedAt < now
                ).GroupBy(en => new
                {
                    en.CreatedAt.Year,
                    en.CreatedAt.Month
                }

                ).Select(group => new
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Count = group.Count()
                }).OrderBy(x => x.Year).
                ThenBy(x => x.Month).
                ToListAsync();



            //Now add the logic for showing the month data even if the enrollment is 0.

            var enrollementData = new List<object>();

            var month = startDate;

            while (month <= startOfMonth)
            {
                var enrollment = activeEnrollment.FirstOrDefault(x =>
                x.Year == month.Year&&
                x.Month == month.Month
                );

                enrollementData.Add(new
                {
                    Year = month.Year,
                    Month = month.Month,
                    Count = enrollment?.Count ?? 0
                });

                month = month.AddMonths(1);
            }

            return Ok(enrollementData);
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
