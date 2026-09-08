using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.SemesterManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles =SystemRoles.superAdmin)]
    public class SemesterController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public SemesterController(ApplicationDbContext context)
        {
            _context = context;
        }


        //Put api for Semester Management

        [HttpPost]
        public IActionResult AddSemester([FromBody] SemesterDTO model)
        {

            
            var semester = new Semester
            {
                SemesterNumber = model.SemesterNumber,
                AcademicProgramId = model.AcademicProgramId,
                AcademicSessionId = model.AcademicSessionId
            };

            // Check whether the semester number is valid or not
            if (model.SemesterNumber <= 0)
            {
                return BadRequest(new { message = "Semester number must be greater than 0" });
            }

            // Check whether the academic program exist or not
            var programExists = _context.AcademicPrograms
                .Any(p => p.Id == model.AcademicProgramId);

            if (!programExists)
            {
                return BadRequest(new { message = "Academic program does not exist" });
            }

            // Check wheher academic session exist or not
            var sessionExists = _context.AcademicSessions
                .Any(s => s.Id == model.AcademicSessionId);

            if (!sessionExists)
            {
                return BadRequest(new { message = "Academic session does not exist" });
            }

            // Check for duplicaiton
            var semesterExists = _context.Semesters.Any(s =>
                s.SemesterNumber == model.SemesterNumber &&
                s.AcademicProgramId == model.AcademicProgramId &&
                s.AcademicSessionId == model.AcademicSessionId);

            if (semesterExists)
            {
                return BadRequest(new { message = "Semester already exists" });
            }

            
            
            _context.Semesters.Add(semester);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Semester added successfully",
                semester.Id
            });
        }

        //Get All Semesters Api

        [HttpGet]
        public IActionResult GetSemesters()
        {
            
            var semesters = _context.Semesters
                .Select(s => new
                {
                    s.Id,
                    s.SemesterNumber,
                    s.AcademicProgramId,
                    ProgramName = s.AcademicProgram.ProgramName,
                    s.AcademicSessionId,
                    SessionName = s.AcademicSession.SessionName
                })
                .ToList();

            return Ok(semesters);
        }

        //GET specific semester by Id

        [HttpGet("{id}")]
        public IActionResult GetSemesterById(int id)
        {
            // Specific semester ko Id se search kar rahe hain
            var semester = _context.Semesters
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.SemesterNumber,
                    s.AcademicProgramId,
                    ProgramName = s.AcademicProgram.ProgramName,
                    s.AcademicSessionId,
                    SessionName = s.AcademicSession.SessionName
                })
                .FirstOrDefault();

            
            if (semester == null)
            {
                return NotFound(new { message = "Semester not found" });
            }

            return Ok(semester);
        }

        //Put Api for semester

        [HttpPut("{id}")]
        public IActionResult UpdateSemester(int id, [FromBody] SemesterDTO model)
        {
            //Check whether the semester exist or not
            var semester = _context.Semesters
                .FirstOrDefault(s => s.Id == id);

            if (semester == null)
            {
                return NotFound(new { message = "Semester not found" });
            }

            //Check Whether the AcademicProgram exist or not
            var programExists = _context.AcademicPrograms
                .Any(p => p.Id == model.AcademicProgramId);

            if (!programExists)
            {
                return BadRequest(new { message = "Academic program does not exist" });
            }

            //Check whether the exist or not
            var sessionExists = _context.AcademicSessions
                .Any(s => s.Id == model.AcademicSessionId);

            if (!sessionExists)
            {
                return BadRequest(new { message = "Academic session does not exist" });
            }

            // Semester Should be valid
            if (model.SemesterNumber <= 0)
            {
                return BadRequest(new { message = "Semester number must be greater than 0" });
            }

            // check duplication
            var duplicateSemester = _context.Semesters.Any(s =>
                s.SemesterNumber == model.SemesterNumber &&
                s.AcademicProgramId == model.AcademicProgramId &&
                s.AcademicSessionId == model.AcademicSessionId &&
                s.Id != id);

            if (duplicateSemester)
            {
                return BadRequest(new { message = "Semester already exists" });
            }

            // Existing semester ki values update kar rahe hain
            semester.SemesterNumber = model.SemesterNumber;
            semester.AcademicProgramId = model.AcademicProgramId;
            semester.AcademicSessionId = model.AcademicSessionId;

            _context.SaveChanges();

            return Ok(new { message = "Semester updated successfully" });
        }

        //Delete Api

        [HttpDelete("{id}")]
        public IActionResult DeleteSemester(int id)
        {
            
            var semester = _context.Semesters
                .FirstOrDefault(s => s.Id == id);

            
            if (semester == null)
            {
                return NotFound(new { message = "Semester not found" });
            }

            
            _context.Semesters.Remove(semester);
            _context.SaveChanges();

            return Ok(new { message = "Semester deleted successfully" });
        }
    }
}
