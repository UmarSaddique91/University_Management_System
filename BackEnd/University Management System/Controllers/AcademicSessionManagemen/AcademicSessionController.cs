using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;
using University_Management_System.Constants;


namespace University_Management_System.Controllers.AcademicSessionManagemen
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles = SystemRoles.superAdmin)]
    public class AcademicSessionController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public AcademicSessionController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Put Request For Sessions

        public IActionResult AddAcademicSession([FromBody] AcademicSessionDTO model)
        {
            if(model.EndDate <= model.StartDate)
            {
                return BadRequest(new { Message = "EndDate must be greater than Start Date" });
            }

            var session = new AcademicSession
            {
                SessionName = model.SessionName,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive
            };

            _context.AcademicSessions.Add(session);
            _context.SaveChanges();

            return Ok(new { message = "Session Added Successfully" });
        }


        //Get Api for Academic Session

        [HttpGet]
        public IActionResult GetAcademicSessions()
        {
            var sessions = _context.AcademicSessions.ToList();

            return Ok(sessions);
        }


        //Get using Id

        [HttpGet("{Id}")]

        public IActionResult GetAcademicSessionById(int Id)
        {
            var session = _context.AcademicSessions.FirstOrDefault(r => r.Id == Id);
            if(session == null){
                return BadRequest(new { message = "Session Not Found " });


                
            }

            return Ok(session);


        }

        //Update API

        [HttpPut("{id}")]
        public IActionResult UpdateAcademicSession(
    int id,
    [FromBody] AcademicSessionDTO model)
        {
            var session = _context.AcademicSessions
                .FirstOrDefault(s => s.Id == id);

            if (session == null)
            {
                return NotFound(new { message = "Academic session not found" });
            }

            if (model.EndDate <= model.StartDate)
            {
                return BadRequest(new
                {
                    message = "End date must be greater than start date"
                });
            }

            session.SessionName = model.SessionName;
            session.StartDate = model.StartDate;
            session.EndDate = model.EndDate;
            session.IsActive = model.IsActive;

            _context.SaveChanges();

            return Ok(new { message = "Academic session updated successfully" });
        }

        //DeleteApi

        [HttpDelete("{id}")]
        public IActionResult DeleteAcademicSession(int id)
        {
            var session = _context.AcademicSessions
                .FirstOrDefault(s => s.Id == id);

            if (session == null)
            {
                return NotFound(new { message = "Academic session not found" });
            }

            _context.AcademicSessions.Remove(session);
            _context.SaveChanges();

            return Ok(new { message = "Academic session deleted successfully" });
        }
    }
}
