using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.Models;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    
    public class RegistrationInvitationController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public RegistrationInvitationController(ApplicationDbContext context)
        {
            _context = context;
        }



        // POST: api/RegistrationInvitation/student/{studentId}
        [HttpPost("student/{studentId}")]
        public async Task<IActionResult> CreateStudentInvitation(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            if (student.Status == "Active")
            {
                return BadRequest(new { message = "Student is already active" });
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            var invitation = new RegistrationInvitation
            {
                Token = token,
                Role = SystemRoles.Student,
                ProfileId = student.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(2),
                IsUsed = false
            };

            _context.RegistrationInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            var registrationLink = $"http://localhost:5173/signUp?token={token}";

            return Ok(new
            {
                message = "Registration invitation created",
                registrationLink
            });
        }

        // GET: api/RegistrationInvitation/validate/{token}
        [HttpGet("validate/{token}")]
        public async Task<IActionResult> ValidateInvitation(string token)
        {
            var invitation = await _context.RegistrationInvitations
                .FirstOrDefaultAsync(i => i.Token == token);

            if (invitation == null)
            {
                return NotFound(new { message = "Invalid registration link" });
            }

            if (invitation.IsUsed)
            {
                return BadRequest(new { message = "Registration link already used" });
            }

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Registration link expired" });
            }

            var student = await _context.Students
                .FindAsync(invitation.ProfileId);

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            return Ok(new
            {
                role = invitation.Role,
                studentName = student.StudentName,
                rollNumber = student.RollNumber,
                recipientEmail = invitation.RecipientEmail
            });
        }
    }
}


