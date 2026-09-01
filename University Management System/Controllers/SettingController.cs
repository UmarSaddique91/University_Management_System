using Microsoft.AspNetCore.Mvc;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;

[Route("api/[controller]")]
[ApiController]
public class SettingController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SettingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetSettings()
    {
        var settings = _context.Settings.FirstOrDefault();
        return Ok(settings);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateSettings(int id, [FromBody] Setting updated)
    {
        var existing = _context.Settings.Find(id);
        if (existing == null)
            return NotFound("Settings not found");

        existing.UniversityName = updated.UniversityName;
        existing.UniversityLogo = updated.UniversityLogo;
        existing.ContactaEmail = updated.ContactaEmail;
        existing.PrimaryAddress = updated.PrimaryAddress;
        existing.DefaultLanguage = updated.DefaultLanguage;
        existing.DefaultTimeZone = updated.DefaultTimeZone;
        existing.DateFormat = updated.DateFormat;
        existing.TeacherModule = updated.TeacherModule;
        existing.CourseModule = updated.CourseModule;
        existing.AssignmentModule = updated.AssignmentModule;
        existing.PaymentModule = updated.PaymentModule;
        existing.PasswordComplexity = updated.PasswordComplexity;
        existing.MinPasswordLength = updated.MinPasswordLength;
        existing.Authentication = updated.Authentication;

        _context.SaveChanges();
        return Ok("Settings updated successfully");
    }
}
