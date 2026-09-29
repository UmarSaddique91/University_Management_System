using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin Access")]
    public class AcademicSessionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AcademicSessionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/AcademicSession
        [HttpPost]
        public async Task<IActionResult> AddAcademicSession([FromBody] AcademicSessionDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.SessionName))
            {
                return BadRequest(new { message = "Session name is required" });
            }

            var cleanName = model.SessionName.Trim();

            // Enforce uniqueness on SessionName
            var nameExists = await _context.AcademicSessions
                .AnyAsync(s => s.SessionName.ToLower() == cleanName.ToLower());

            if (nameExists)
            {
                return BadRequest(new { message = $"An academic session with name '{cleanName}' already exists." });
            }

            if (model.EndDate <= model.StartDate)
            {
                return BadRequest(new { message = "EndDate must be greater than Start Date" });
            }

            // Enforce single active session rule: activating this session deactivates any other active session
            if (model.IsActive)
            {
                var otherActiveSessions = await _context.AcademicSessions
                    .Where(s => s.IsActive)
                    .ToListAsync();

                foreach (var activeSession in otherActiveSessions)
                {
                    activeSession.IsActive = false;
                }
            }

            var session = new AcademicSession
            {
                SessionName = cleanName,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive
            };

            _context.AcademicSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Session Added Successfully", id = session.Id });
        }

        // GET: api/AcademicSession
        [HttpGet]
        public async Task<IActionResult> GetAcademicSessions()
        {
            var sessions = await _context.AcademicSessions
                .OrderByDescending(s => s.IsActive)
                .ThenByDescending(s => s.StartDate)
                .ToListAsync();

            return Ok(sessions);
        }

        // GET: api/AcademicSession/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAcademicSessionById(int id)
        {
            var session = await _context.AcademicSessions.FirstOrDefaultAsync(r => r.Id == id);
            if (session == null)
            {
                return NotFound(new { message = "Session Not Found" });
            }

            return Ok(session);
        }

        // PUT: api/AcademicSession/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAcademicSession(int id, [FromBody] AcademicSessionDTO model)
        {
            var session = await _context.AcademicSessions.FirstOrDefaultAsync(s => s.Id == id);
            if (session == null)
            {
                return NotFound(new { message = "Academic session not found" });
            }

            if (string.IsNullOrWhiteSpace(model.SessionName))
            {
                return BadRequest(new { message = "Session name is required" });
            }

            var cleanName = model.SessionName.Trim();

            // Enforce uniqueness on SessionName (excluding self)
            var nameExists = await _context.AcademicSessions
                .AnyAsync(s => s.SessionName.ToLower() == cleanName.ToLower() && s.Id != id);

            if (nameExists)
            {
                return BadRequest(new { message = $"An academic session with name '{cleanName}' already exists." });
            }

            if (model.EndDate <= model.StartDate)
            {
                return BadRequest(new { message = "End date must be greater than start date" });
            }

            // Enforce single active session rule
            if (model.IsActive)
            {
                var otherActiveSessions = await _context.AcademicSessions
                    .Where(s => s.IsActive && s.Id != id)
                    .ToListAsync();

                foreach (var activeSession in otherActiveSessions)
                {
                    activeSession.IsActive = false;
                }
            }

            session.SessionName = cleanName;
            session.StartDate = model.StartDate;
            session.EndDate = model.EndDate;
            session.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Academic session updated successfully" });
        }

        // DELETE: api/AcademicSession/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAcademicSession(int id)
        {
            var session = await _context.AcademicSessions.FirstOrDefaultAsync(s => s.Id == id);
            if (session == null)
            {
                return NotFound(new { message = "Academic session not found" });
            }

            // Check if any Semesters reference this Session
            var hasSemesters = await _context.Semesters.AnyAsync(s => s.AcademicSessionId == id);
            if (hasSemesters)
            {
                return BadRequest(new
                {
                    message = "Cannot delete academic session because it is referenced by existing semesters. Deactivate it instead."
                });
            }

            _context.AcademicSessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Academic session deleted successfully" });
        }
    }
}


