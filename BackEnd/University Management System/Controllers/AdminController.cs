using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = SystemRoles.superAdmin)]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Admin
        [HttpGet]
        public async Task<IActionResult> GetAdmins()
        {
            var admins = await _context.AdminProfiles
                .OrderByDescending(a => a.Id)
                .Select(a => new
                {
                    a.Id,
                    a.AdminId,
                    a.AdminName,
                    a.Email,
                    a.Status,
                    a.UserId,
                    IsRegistered = !string.IsNullOrEmpty(a.UserId),
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(admins);
        }

        // POST: api/Admin
        [HttpPost]
        public async Task<IActionResult> PreRegisterAdmin([FromBody] AdminProfileDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trimmedName = model.AdminName.Trim();
            var trimmedEmail = model.Email.Trim().ToLower();

            // Check email across Identity users, AdminProfiles, and Teachers
            var identityUserExists = await _userManager.FindByEmailAsync(trimmedEmail);
            if (identityUserExists != null)
            {
                return BadRequest(new { message = "An account with this email is already registered." });
            }

            var adminExists = await _context.AdminProfiles
                .AnyAsync(a => a.Email.ToLower() == trimmedEmail);
            if (adminExists)
            {
                return BadRequest(new { message = "An admin profile with this email already exists." });
            }

            var teacherExists = await _context.Teachers
                .AnyAsync(t => t.Email != null && t.Email.ToLower() == trimmedEmail);
            if (teacherExists)
            {
                return BadRequest(new { message = "A teacher account with this email already exists." });
            }

            // Generate unique Admin ID
            var count = await _context.AdminProfiles.CountAsync();
            var adminId = $"ADM-{1001 + count}";

            var adminProfile = new AdminProfile
            {
                AdminId = adminId,
                AdminName = trimmedName,
                Email = trimmedEmail,
                Status = "Inactive",
                UserId = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.AdminProfiles.Add(adminProfile);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Admin profile pre-registered successfully. The user can now register at /signUp.",
                adminProfile.Id,
                adminProfile.AdminId
            });
        }

        // PUT: api/Admin/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(int id, [FromBody] AdminProfileDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var adminProfile = await _context.AdminProfiles.FirstOrDefaultAsync(a => a.Id == id);
            if (adminProfile == null)
            {
                return NotFound(new { message = "Admin profile not found" });
            }

            var trimmedName = model.AdminName.Trim();
            var trimmedEmail = model.Email.Trim().ToLower();

            // Registered admin: Email is locked
            if (!string.IsNullOrEmpty(adminProfile.UserId))
            {
                if (!string.Equals(adminProfile.Email, trimmedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message = "Email cannot be changed for an already registered admin account. Only the name can be updated."
                    });
                }

                adminProfile.AdminName = trimmedName;
            }
            else
            {
                // Pending admin: Validate email changes against duplicates
                if (!string.Equals(adminProfile.Email, trimmedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    var identityUserExists = await _userManager.FindByEmailAsync(trimmedEmail);
                    if (identityUserExists != null)
                    {
                        return BadRequest(new { message = "An account with this email is already registered." });
                    }

                    var adminExists = await _context.AdminProfiles
                        .AnyAsync(a => a.Email.ToLower() == trimmedEmail && a.Id != id);
                    if (adminExists)
                    {
                        return BadRequest(new { message = "Another admin profile with this email already exists." });
                    }

                    var teacherExists = await _context.Teachers
                        .AnyAsync(t => t.Email != null && t.Email.ToLower() == trimmedEmail);
                    if (teacherExists)
                    {
                        return BadRequest(new { message = "A teacher account with this email already exists." });
                    }

                    adminProfile.Email = trimmedEmail;
                }

                adminProfile.AdminName = trimmedName;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin profile updated successfully" });
        }

        // PATCH: api/Admin/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] StatusUpdateDTO? model)
        {
            var adminProfile = await _context.AdminProfiles.FirstOrDefaultAsync(a => a.Id == id);
            if (adminProfile == null)
            {
                return NotFound(new { message = "Admin profile not found" });
            }

            // Unregistered admin cannot be activated
            if (string.IsNullOrEmpty(adminProfile.UserId))
            {
                return BadRequest(new
                {
                    message = "Admin account cannot be activated before registration. The administrator must complete registration at /signUp first."
                });
            }

            var nextStatus = !string.IsNullOrWhiteSpace(model?.Status)
                ? model.Status.Trim()
                : (adminProfile.Status == "Active" ? "Inactive" : "Active");

            adminProfile.Status = nextStatus;

            // Sync with IdentityUser lockout and security stamp
            var user = await _userManager.FindByIdAsync(adminProfile.UserId);
            if (user != null)
            {
                if (nextStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                }
                else
                {
                    user.LockoutEnd = null;
                }

                await _userManager.UpdateSecurityStampAsync(user);
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Admin account set to {nextStatus}",
                status = nextStatus
            });
        }

        // DELETE: api/Admin/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var adminProfile = await _context.AdminProfiles.FirstOrDefaultAsync(a => a.Id == id);
            if (adminProfile == null)
            {
                return NotFound(new { message = "Admin profile not found" });
            }

            // If an associated Identity user exists, delete the user account
            IdentityUser? user = null;
            if (!string.IsNullOrEmpty(adminProfile.UserId))
            {
                user = await _userManager.FindByIdAsync(adminProfile.UserId);
            }
            if (user == null && !string.IsNullOrEmpty(adminProfile.Email))
            {
                user = await _userManager.FindByEmailAsync(adminProfile.Email);
            }

            if (user != null)
            {
                var isSuper = await _userManager.IsInRoleAsync(user, SystemRoles.superAdmin);
                if (isSuper)
                {
                    return BadRequest(new { message = "Super Administrator account cannot be deleted." });
                }

                var deleteUserResult = await _userManager.DeleteAsync(user);
                if (!deleteUserResult.Succeeded)
                {
                    var errors = string.Join(", ", deleteUserResult.Errors.Select(e => e.Description));
                    return BadRequest(new { message = $"Failed to delete associated user account: {errors}" });
                }
            }

            _context.AdminProfiles.Remove(adminProfile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Administrator deleted successfully" });
        }
    }
}

