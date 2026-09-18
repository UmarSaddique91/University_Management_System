using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;
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
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
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

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if(existingUser != null)
            {
                return BadRequest(new { message = "User Already Registered" });
            }
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleResult=await _userManager.AddToRoleAsync(user, SystemRoles.Student);

                if (!roleResult.Succeeded)
                {
                    return BadRequest(new { message = (roleResult.Errors) });
                }



                //Add User in Student Table


                var student = new Student
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Status = "Active"
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                return Ok(new { message = "User Registered Successfully!" });
            }

           

            return BadRequest(result.Errors);
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