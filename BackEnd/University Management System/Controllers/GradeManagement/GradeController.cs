using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.GradeManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles =SystemRoles.superAdmin)]
    public class GradeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GradeController(ApplicationDbContext context)
        {
            _context = context;
        }


        //Put API for Grades Management


        [HttpPost]
        public IActionResult AddGrade([FromBody] GradeDTO model)
        {
            // Check Whther the Student Existis or not
            var studentExists = _context.Students
                .Any(s => s.Id == model.StudentId);

            if (!studentExists)
            {
                return BadRequest(new { message = "Student does not exist" });
            }




            //Fetch the Required Exam from the Examns Table
            var exam = _context.Exams
                .FirstOrDefault(e => e.Id == model.ExamId);

            if (exam == null)
            {
                return BadRequest(new { message = "Exam does not exist" });
            }



            // Check the Validity Of Obtained Marks
            if (model.ObtainedMarks < 0 || model.ObtainedMarks > exam.TotalMarks)
            {
                return BadRequest(new
                {
                    message = "Obtained marks must be between 0 and exam total marks"
                });
            }



            // Check the duplicacy of student and exam
            var duplicateGrade = _context.Grades.Any(g =>
                g.StudentId == model.StudentId &&
                g.ExamId == model.ExamId);

            if (duplicateGrade)
            {
                return BadRequest(new
                {
                    message = "Grade already exists for this student and exam"
                });
            }

            // DTO se Grade entity create kar rahe hain
            var grade = new Grade
            {
                StudentId = model.StudentId,
                ExamId = model.ExamId,
                ObtainedMarks = model.ObtainedMarks,
                GradeLetter = model.GradeLetter
            };

            _context.Grades.Add(grade);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Grade added successfully",
                grade.Id
            });
        }



        //Get Api for getting all exams data

        [HttpGet]
        public IActionResult GetGrades()
        {
            // Grades ko readable details ke sath fetch kar rahe hain
            var grades = _context.Grades
                .Select(g => new
                {
                    g.Id,

                    g.StudentId,
                    StudentName = g.Student.StudentName,

                    g.ExamId,
                    ExamType = g.Exam.ExamType,
                    CourseName = g.Exam.CourseOffering.Course.CourseName,

                    g.ObtainedMarks,
                    TotalMarks = g.Exam.TotalMarks,
                    g.GradeLetter
                })
                .ToList();

            return Ok(grades);
        }



        //Get Specific Exam Detail by Id

        [HttpGet("{id}")]
        public IActionResult GetGradeById(int id)
        {
            
            var grade = _context.Grades
                .Where(g => g.Id == id)
                .Select(g => new
                {
                    g.Id,

                    g.StudentId,
                    StudentName = g.Student.StudentName,

                    g.ExamId,
                    ExamType = g.Exam.ExamType,
                    CourseName = g.Exam.CourseOffering.Course.CourseName,

                    g.ObtainedMarks,
                    TotalMarks = g.Exam.TotalMarks,
                    g.GradeLetter
                })
                .FirstOrDefault();

            if (grade == null)
            {
                return NotFound(new { message = "Grade not found" });
            }

            return Ok(grade);
        }


        //Put API for Updating Grade


        [HttpPut("{id}")]
        public IActionResult UpdateGrade(int id, [FromBody] GradeDTO model)
        {
            // Find the Existing Grdes Data
            var grade = _context.Grades
                .FirstOrDefault(g => g.Id == id);

            if (grade == null)
            {
                return NotFound(new { message = "Grade not found" });
            }




            // Check the Validity Of Student
            var studentExists = _context.Students
                .Any(s => s.Id == model.StudentId);

            if (!studentExists)
            {
                return BadRequest(new { message = "Student does not exist" });
            }



            // Fetching and Vlidatig the Exams for Grade
            var exam = _context.Exams
                .FirstOrDefault(e => e.Id == model.ExamId);

            if (exam == null)
            {
                return BadRequest(new { message = "Exam does not exist" });
            }



            // Validaity of Obtained Marks
            if (model.ObtainedMarks < 0 || model.ObtainedMarks > exam.TotalMarks)
            {
                return BadRequest(new
                {
                    message = "Obtained marks must be between 0 and exam total marks"
                });
            }

            // Check the Duplicacy
            var duplicateGrade = _context.Grades.Any(g =>
                g.StudentId == model.StudentId &&
                g.ExamId == model.ExamId &&
                g.Id != id);

            if (duplicateGrade)
            {
                return BadRequest(new
                {
                    message = "Grade already exists for this student and exam"
                });
            }

            
            grade.StudentId = model.StudentId;
            grade.ExamId = model.ExamId;
            grade.ObtainedMarks = model.ObtainedMarks;
            grade.GradeLetter = model.GradeLetter;

            _context.SaveChanges();

            return Ok(new { message = "Grade updated successfully" });
        }


        //Delete API for exam


        [HttpDelete("{id}")]
        public IActionResult DeleteGrade(int id)
        {
            //Find the Grade
            var grade = _context.Grades
                .FirstOrDefault(g => g.Id == id);

            if (grade == null)
            {
                return NotFound(new { message = "Grade not found" });
            }

            
            _context.Grades.Remove(grade);
            _context.SaveChanges();

            return Ok(new { message = "Grade deleted successfully" });
        }
    }
}
