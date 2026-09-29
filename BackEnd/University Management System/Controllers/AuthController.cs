using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models;
using University_Management_System.Services;


namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        private readonly ApplicationDbContext _context;

        public AuthController(UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            EmailService emailService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _context = context;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {

                //Get Assigned role of User

                var userRoles = await _userManager.GetRolesAsync(user);

                // Determine user display name
                string displayName = "Admin";
                if (userRoles.Contains(SystemRoles.superAdmin))
                {
                    displayName = "Super Admin";
                }
                else if (userRoles.Contains(SystemRoles.Admin))
                {
                    var adminProfile = await _context.AdminProfiles
                        .FirstOrDefaultAsync(a => a.UserId == user.Id || (a.Email != null && a.Email.ToLower() == user.Email.ToLower()));

                    if (adminProfile != null)
                    {
                        if (!adminProfile.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                        {
                            return Unauthorized(new { message = "Your admin account is inactive. Please contact the Super Administrator." });
                        }

                        if (!string.IsNullOrWhiteSpace(adminProfile.AdminName))
                        {
                            displayName = adminProfile.AdminName;
                        }
                    }
                }
                else if (userRoles.Contains(SystemRoles.Teacher))
                {
                    var teacher = await _context.Teachers
                        .FirstOrDefaultAsync(t => t.UserId == user.Id || (t.Email != null && t.Email.ToLower() == user.Email.ToLower()));
                    if (teacher != null && !string.IsNullOrWhiteSpace(teacher.TeacherName))
                    {
                        displayName = teacher.TeacherName;
                    }
                }
                else if (userRoles.Contains(SystemRoles.Student))
                {
                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.UserId == user.Id || (s.Email != null && s.Email.ToLower() == user.Email.ToLower()));
                    if (student != null && !string.IsNullOrWhiteSpace(student.StudentName))
                    {
                        displayName = student.StudentName;
                    }
                }

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };


                foreach(var role in userRoles){
                    authClaims.Add(
                        new Claim(ClaimTypes.Role, role)
                        );
                }
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    expires: DateTime.Now.AddHours(8),
                    claims: authClaims,
                    signingCredentials : new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    roles = userRoles,
                    name = displayName,
                    message="Login Successfull"

                });

                
            }

            return Unauthorized(new { message = "Invalid email or password." });
        }


        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(model.Token))
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return BadRequest(new { message = "Email is required" });
                }

                var normalizedEmail = model.Email.Trim().ToLower();

                // 1. Check if pending Admin account exists
                var pendingAdmin = await _context.AdminProfiles
                    .FirstOrDefaultAsync(a => a.Email != null && a.Email.ToLower() == normalizedEmail && a.UserId == null);

                if (pendingAdmin != null)
                {
                    var existingAdminUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingAdminUser != null)
                    {
                        return BadRequest(new { message = "User already registered" });
                    }

                    var adminUser = new IdentityUser { UserName = model.Email, Email = model.Email };
                    var createResult = await _userManager.CreateAsync(adminUser, model.Password);
                    if (!createResult.Succeeded)
                    {
                        return BadRequest(createResult.Errors);
                    }

                    var adminRoleResult = await _userManager.AddToRoleAsync(adminUser, SystemRoles.Admin);
                    if (!adminRoleResult.Succeeded)
                    {
                        return BadRequest(new { message = adminRoleResult.Errors });
                    }

                    pendingAdmin.UserId = adminUser.Id;
                    pendingAdmin.Status = "Active";
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Administrator registered and account activated successfully!" });
                }

                // 2. Check if pending Teacher account exists
                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.Email != null && t.Email.ToLower() == normalizedEmail && t.UserId == null && t.Status == "Inactive");

                if (teacher != null)
                {
                    var existingTeacherUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingTeacherUser != null)
                    {
                        return BadRequest(new { message = "User already registered" });
                    }

                    var teacherUser = new IdentityUser { UserName = model.Email, Email = model.Email };
                    var createResult = await _userManager.CreateAsync(teacherUser, model.Password);

                    if (!createResult.Succeeded)
                    {
                        return BadRequest(createResult.Errors);
                    }

                    var teacherRoleResult = await _userManager.AddToRoleAsync(teacherUser, SystemRoles.Teacher);
                    if (!teacherRoleResult.Succeeded)
                    {
                        return BadRequest(new { message = teacherRoleResult.Errors });
                    }

                    teacher.UserId = teacherUser.Id;
                    teacher.Status = "Active";

                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Teacher registered and account activated successfully!" });
                }

                return BadRequest(new { message = "No pending Administrator or Teacher account found for this email. Please contact the administrator." });
            }

            var invitation = await _context.RegistrationInvitations
                .FirstOrDefaultAsync(i => i.Token == model.Token);

            if (invitation == null)
            {
                return BadRequest(new { message = "Invalid registration link" });
            }

            if (invitation.IsUsed)
            {
                return BadRequest(new { message = "Registration link already used" });
            }

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Registration link expired" });
            }

            // Case-insensitive comparison with invitation recipient email
            if (!string.Equals(model.Email?.Trim(), invitation.RecipientEmail?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Email does not match the invitation email" });
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already registered" });
            }

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign role from the invitation
            var roleToAssign = string.IsNullOrWhiteSpace(invitation.Role) ? SystemRoles.Student : invitation.Role;
            var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);

            if (!roleResult.Succeeded)
            {
                return BadRequest(new { message = roleResult.Errors });
            }

            // Find existing Student by ProfileId and activate account
            var student = await _context.Students.FindAsync(invitation.ProfileId);
            if (student != null)
            {
                student.UserId = user.Id;
                student.Email = model.Email;
                student.Status = "Active";
            }

            invitation.IsUsed = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User Registered Successfully!" });
        }



        //Post Request for forgot Password

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Ok(new
                {
                    message = "User Not Found"
                });
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(resetToken);
            var encodedEmail = Uri.EscapeDataString(user.Email!);

            var resetLink =
                $"http://localhost:5173/resetPassword?email={encodedEmail}&token={encodedToken}";


            var subject = "Reset Your Password";

            var body = $@"
    <h2>Password Reset</h2>
    <p>You requested to reset your password.</p>
    <p>Click the link below to reset it:</p>
    <a href='{resetLink}'>Reset Password</a>
";

            await _emailService.SendEmailAsync(
                user.Email!,
                subject,
                body
            );


            return Ok(new
            {
                message = "Reset Link Sent Successfully"
            });
        }





        //Post Request for Reset Password


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return BadRequest(new
                {
                    message = "Invalid password reset request"
                });
            }

            
            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Password could not be reset",
                    errors = result.Errors
                });
            }

            return Ok(new
            {
                message = "Password reset successfully"
            });
        }
    }

  
}

