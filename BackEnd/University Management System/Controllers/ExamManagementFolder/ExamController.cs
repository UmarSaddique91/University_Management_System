using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.ExamManagementFolder
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles =SystemRoles.superAdmin)]
    public class ExamController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public ExamController(ApplicationDbContext context)
        {
            _context = context;
        }



        //Post API for Exam Controller

        [HttpPost]
        public IActionResult AddExam([FromBody] ExamDTO model)
        {
            // Check the validation of course offering
            var courseOfferingExists = _context.CourseOfferings
                .Any(co => co.Id == model.CourseOfferingId);

            if (!courseOfferingExists)
            {
                return BadRequest(new { message = "Course offering does not exist" });
            }



            // Check the validity of marks
            if (model.TotalMarks <= 0)
            {
                return BadRequest(new { message = "Total marks must be greater than 0" });
            }



            // Check the duplicacy of exam
            var duplicateExam = _context.Exams.Any(e =>
                e.CourseOfferingId == model.CourseOfferingId &&
                e.ExamType == model.ExamType&&
                e.ExamDate.Date == model.ExamDate.Date);

            if (duplicateExam)
            {
                return BadRequest(new { message = "Exam already exists for this course offering" });
            }

            
            var exam = new Exam
            {
                CourseOfferingId = model.CourseOfferingId,
                ExamType = model.ExamType,
                ExamDate = model.ExamDate,
                TotalMarks = model.TotalMarks
            };

            
            _context.Exams.Add(exam);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Exam added successfully",
                exam.Id
            });
        }



        //Get Api for Exam controller

        [HttpGet]
        public IActionResult GetExams()
        {
            
            var exams = _context.Exams
                .Select(e => new
                {
                    e.Id,
                    e.CourseOfferingId,

                    CourseName = e.CourseOffering.Course.CourseName,
                    TeacherName = e.CourseOffering.Teacher.TeacherName,
                    SemesterNumber = e.CourseOffering.Semester.SemesterNumber,

                    e.ExamType,
                    e.ExamDate,
                    e.TotalMarks
                })
                .ToList();

            return Ok(exams);
        }


        //Get specific Exam Detail

        [HttpGet("{id}")]
        public IActionResult GetExamById(int id)
        {
            
            var exam = _context.Exams
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.CourseOfferingId,

                    CourseName = e.CourseOffering.Course.CourseName,
                    TeacherName = e.CourseOffering.Teacher.TeacherName,
                    SemesterNumber = e.CourseOffering.Semester.SemesterNumber,

                    e.ExamType,
                    e.ExamDate,
                    e.TotalMarks
                })
                .FirstOrDefault();

            if (exam == null)
            {
                return NotFound(new { message = "Exam not found" });
            }

            return Ok(exam);
        }


        //Put Api for Exam Controller


        [HttpPut("{id}")]
        public IActionResult UpdateExam(int id, [FromBody] ExamDTO model)
        {
            // Find the exisitng Exam for update by Id
            var exam = _context.Exams
                .FirstOrDefault(e => e.Id == id);

            if (exam == null)
            {
                return NotFound(new { message = "Exam not found" });
            }




            // Check whether the course exist or not
            var courseOfferingExists = _context.CourseOfferings
                .Any(co => co.Id == model.CourseOfferingId);

            if (!courseOfferingExists)
            {
                return BadRequest(new { message = "Course offering does not exist" });
            }




            // Cehck the Validity Of Marks
            if (model.TotalMarks <= 0)
            {
                return BadRequest(new { message = "Total marks must be greater than 0" });
            }




            // Check the duplication of Exam
            var duplicateExam = _context.Exams.Any(e =>
                e.CourseOfferingId == model.CourseOfferingId &&
                e.ExamType == model.ExamType &&
                e.ExamDate.Date == model.ExamDate.Date &&
                e.Id != id);

            if (duplicateExam)
            {
                return BadRequest(new
                {
                    message = "Exam already exists for this course offering and date"
                });
            }

            
            exam.CourseOfferingId = model.CourseOfferingId;
            exam.ExamType = model.ExamType;
            exam.ExamDate = model.ExamDate;
            exam.TotalMarks = model.TotalMarks;

            _context.SaveChanges();

            return Ok(new { message = "Exam updated successfully" });
        }



        //Delete Api for Exam Management


        [HttpDelete("{id}")]
        public IActionResult DeleteExam(int id)
        {
            
            var exam = _context.Exams
                .FirstOrDefault(e => e.Id == id);

            if (exam == null)
            {
                return NotFound(new { message = "Exam not found" });
            }

            
            _context.Exams.Remove(exam);
            _context.SaveChanges();

            return Ok(new { message = "Exam deleted successfully" });
        }
    }
}
