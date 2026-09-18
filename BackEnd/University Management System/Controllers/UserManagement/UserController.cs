using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.UserManagement
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize(Roles = SystemRoles.superAdmin)]
    public class UserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext _context;

        public UserController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole>roleManager,
            ApplicationDbContext context
            )
        {
            _userManager = userManager;
            _roleManager=roleManager;
            _context = context;
        }


        //Get Request

        [HttpGet]

        public IActionResult GetUsers()
        {
            var users = _userManager.Users.Select(user => new
            {
                user.Id,
                user.UserName,
                user.Email
            }).ToList();

            return Ok(users);
        }


        //For Finding Roles Of Specifc Users By Id

        [HttpGet("{userId}/roles")]

        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "User Not Found" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.Email,
                Roles=roles
            });
        }





        //For Assigning Roles

        [HttpPost("{userId}/roles/{roleName}")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            // Find User
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "User Not Found" });
            }


            // Check Role Exists
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest(new { message = "Role not Found" });
            }


            // Get Current Role
            var currentRoles = await _userManager.GetRolesAsync(user);


            // Remove Previous Role
            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRoleAsync(
                    user,
                    currentRoles[0]
                );
            }


            // Assign New Role
            var result = await _userManager.AddToRoleAsync(user, roleName);


            // First confirm role was assigned successfully
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }


            // If new role is Teacher
            if (roleName == SystemRoles.Teacher)
            {
                // Check if Teacher profile already exists
                var teacherExist = _context.Teachers
                    .Any(t => t.UserId == user.Id);


                // Find Student profile linked with same Identity User
                var student = _context.Students
                    .FirstOrDefault(s => s.UserId == user.Id);


                // Remove temporary Student profile
                if (student != null)
                {
                    _context.Students.Remove(student);
                }


                // Create Teacher profile if it doesn't already exist
                if (!teacherExist)
                {
                    var teacher = new Teacher
                    {
                        UserId = user.Id,
                        Email = user.Email
                    };

                    _context.Teachers.Add(teacher);
                }


                // Save Student delete + Teacher add together
                await _context.SaveChangesAsync();
            }


            return Ok(new
            {
                message = $"Role {roleName} assigned successfully"
            });
        }




        //For removing role

        [HttpDelete("{userId}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "User Not Found" });
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest(new { message = "Role does not exist" });
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                return BadRequest(new { message = "User does not have this role" });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = $"Role {roleName} removed successfully" });
        }
    }
}
