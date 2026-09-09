using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Identity.Client;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.AttendanceManagementController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles = SystemRoles.superAdmin)]
    public class AttendanceController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
            _context = context;
        }

        //Put Api for Attendance

        public IActionResult AddAttendence([FromBody] AttendanceDTO model)
        {
            //Check student Validity

            var studentExist = _context.Students.Any(st => st.Id == model.StudentId);

            if (!studentExist)
            {
                return BadRequest(new { message = "Student did not exist" });
            }


            //Check Course Validity

            var courseExist = _context.CourseOfferings.Any(co => co.Id == model.CourseOfferingId);

            if (!courseExist)
            {
                return BadRequest("Course Did not Found");
            }


            //Check whether the student is enrolled in specific course or not

            var isEnrolled = _context.Enrollments.Any(en =>
            en.StudentId == model.StudentId &&
            en.CourseOfferingId == model.CourseOfferingId
            );

            if (!isEnrolled)
            {
                return BadRequest(new
                {
                    message = "Student is not enrolled in this course offering"
                });
            }

            // Check Duplicacy of Attendance by Student
            var duplicateAttendance = _context.Attendance.Any(a =>
                a.StudentId == model.StudentId &&
                a.CourseOfferingId == model.CourseOfferingId &&
                a.AttendanceDate.Date == model.AttendanceDate.Date);

            if (duplicateAttendance)
            {
                return BadRequest(new
                {
                    message = "Attendance already marked for this date"
                });
            }

            
            var attendance = new Attendance
            {
                StudentId = model.StudentId,
                CourseOfferingId = model.CourseOfferingId,
                AttendanceDate = model.AttendanceDate,
                Status = model.Status
            };


            _context.Attendance.Add(attendance);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Attendance marked successfully",
                attendance.Id
            });

        }


        //Get All Students

        [HttpGet]
        public IActionResult GetAttendances()
        {
            
            var attendances = _context.Attendance
                .Select(a => new
                {
                    a.Id,
                    a.StudentId,
                    StudentName = a.Student.StudentName,

                    a.CourseOfferingId,
                    CourseName = a.CourseOffering.Course.CourseName,

                    a.AttendanceDate,
                    a.Status
                })
                .ToList();

            return Ok(attendances);
        }



        //Get Attendence by Id

        [HttpGet("{id}")]
        public IActionResult GetAttendanceById(int id)
        {
            
            var attendance = _context.Attendance
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.StudentId,
                    StudentName = a.Student.StudentName,

                    a.CourseOfferingId,
                    CourseName = a.CourseOffering.Course.CourseName,

                    a.AttendanceDate,
                    a.Status
                })
                .FirstOrDefault();

            if (attendance == null)
            {
                return NotFound(new { message = "Attendance record not found" });
            }

            return Ok(attendance);
        }


        //Update Attendence record

        [HttpPut("{id}")]
        public IActionResult UpdateAttendance(int id, [FromBody] AttendanceDTO model)
        {
            //Find the given id in the existence attandence Table
            var attendance = _context.Attendance
                .FirstOrDefault(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound(new { message = "Attendance record not found" });
            }




            // Check the validity of Student
            var studentExists = _context.Students
                .Any(s => s.Id == model.StudentId);

            if (!studentExists)
            {
                return BadRequest(new { message = "Student does not exist" });
            }




            // Check the validity of Course
            var offeringExists = _context.CourseOfferings
                .Any(co => co.Id == model.CourseOfferingId);

            if (!offeringExists)
            {
                return BadRequest(new { message = "Course offering does not exist" });
            }




            // Check whether ths stuedetn is enroll in given course
            var isEnrolled = _context.Enrollments.Any(e =>
                e.StudentId == model.StudentId &&
                e.CourseOfferingId == model.CourseOfferingId);

            if (!isEnrolled)
            {
                return BadRequest(new
                {
                    message = "Student is not enrolled in this course offering"
                });
            }




            // Check the duplicacy of Attendence
            var duplicateAttendance = _context.Attendance.Any(a =>
                a.StudentId == model.StudentId &&
                a.CourseOfferingId == model.CourseOfferingId &&
                a.AttendanceDate.Date == model.AttendanceDate.Date &&
                a.Id != id);

            if (duplicateAttendance)
            {
                return BadRequest(new
                {
                    message = "Attendance already marked for this date"
                });
            }




            // Existing record update
            attendance.StudentId = model.StudentId;
            attendance.CourseOfferingId = model.CourseOfferingId;
            attendance.AttendanceDate = model.AttendanceDate;
            attendance.Status = model.Status;

            _context.SaveChanges();

            return Ok(new { message = "Attendance updated successfully" });
        }


        //Delete Attendnce

        [HttpDelete("{id}")]
        public IActionResult DeleteAttendance(int id)
        {
            // Attendance record find kar rahe hain
            var attendance = _context.Attendance
                .FirstOrDefault(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound(new { message = "Attendance record not found" });
            }

            // Record database se delete kar rahe hain
            _context.Attendance.Remove(attendance);
            _context.SaveChanges();

            return Ok(new { message = "Attendance deleted successfully" });
        }
    }
}
