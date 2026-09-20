using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.RegistrationInvitationController
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



        //Post Method

        [HttpPost("student/{studentId}")]
        public async Task<IActionResult> CreateStudentInvitation(int studentId)
        {
            // Student row find karo
            var student = await _context.Students.FindAsync(studentId);

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            // Active student ko dobara invitation nahi deni
            if (student.Status == "Active")
            {
                return BadRequest(new
                {
                    message = "Student is already active"
                });
            }

            // Secure random token
            var token = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32)
            );

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

            var registrationLink =
                $"http://localhost:5173/signUp?token={token}";

            return Ok(new
            {
                message = "Registration invitation created",
                registrationLink
            });
        }










        //Get Api



        [HttpGet("validate/{token}")]
        public async Task<IActionResult> ValidateInvitation(string token)
        {
            var invitation = await _context.RegistrationInvitations
                .FirstOrDefaultAsync(i => i.Token == token);



            //Validate Invitation Link

            if (invitation == null)
            {
                return NotFound(new { message = "Invalid registration link" });
            }


            //Check if it is already used or not

            if (invitation.IsUsed)
            {
                return BadRequest(new { message = "Registration link already used" });
            }



            //Validate the expiration

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
