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
    public class AcademicProgramController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AcademicProgramController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AcademicProgram
        [HttpGet]
        public async Task<IActionResult> GetAcademicPrograms([FromQuery] int? sessionId)
        {
            var activeSession = sessionId.HasValue && sessionId.Value > 0
                ? await _context.AcademicSessions.FirstOrDefaultAsync(s => s.Id == sessionId.Value)
                : await _context.AcademicSessions.FirstOrDefaultAsync(s => s.IsActive);

            var activeSessionId = activeSession?.Id;

            var programs = await _context.AcademicPrograms
                .Select(program => new
                {
                    program.Id,
                    program.ProgramName,
                    program.ProgramCode,
                    program.DurationInSemesters,
                    program.DepartmentId,
                    DepartmentName = program.Department != null
                        ? program.Department.DepartmentName
                        : null,
                    ActiveSessionId = activeSessionId,
                    ActiveSessionName = activeSession != null ? activeSession.SessionName : null,


                    ConfiguredSemestersCount = activeSessionId.HasValue
                        ? _context.Semesters.Count(s => s.AcademicProgramId == program.Id && s.AcademicSessionId == activeSessionId.Value)
                        : _context.Semesters.Where(s => s.AcademicProgramId == program.Id).Select(s => s.SemesterNumber).Distinct().Count()
                })
                .ToListAsync();

            return Ok(programs);
        }

        // GET: api/AcademicProgram/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAcademicProgramById(int id)
        {
            var program = await _context.AcademicPrograms
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.ProgramName,
                    p.ProgramCode,
                    p.DurationInSemesters,
                    p.DepartmentId,
                    DepartmentName = p.Department != null
                        ? p.Department.DepartmentName
                        : null
                })
                .FirstOrDefaultAsync();

            if (program == null)
            {
                return NotFound(new { message = "Academic program not found" });
            }

            return Ok(program);
        }

        // POST: api/AcademicProgram
        [HttpPost]
        public async Task<IActionResult> AddAcademicProgram([FromBody] AcademicProgramDTO model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Program data is invalid" });
            }

            if (string.IsNullOrWhiteSpace(model.ProgramName))
            {
                return BadRequest(new { message = "Program name is required" });
            }

            if (string.IsNullOrWhiteSpace(model.ProgramCode))
            {
                return BadRequest(new { message = "Program code is required" });
            }

            if (!model.DurationInSemesters.HasValue || model.DurationInSemesters <= 0)
            {
                return BadRequest(new { message = "Duration in semesters must be a positive number" });
            }

            if (!model.DepartmentId.HasValue)
            {
                return BadRequest(new { message = "Department is required" });
            }

            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == model.DepartmentId.Value);

            if (!departmentExists)
            {
                return BadRequest(new { message = "Department does not exist" });
            }

            var trimmedCode = model.ProgramCode.Trim();
            var duplicateProgram = await _context.AcademicPrograms
                .AnyAsync(p => p.ProgramCode != null && p.ProgramCode.ToLower() == trimmedCode.ToLower());

            if (duplicateProgram)
            {
                return BadRequest(new { message = "Program code already exists" });
            }

            var academicProgram = new AcademicProgram
            {
                ProgramName = model.ProgramName.Trim(),
                ProgramCode = trimmedCode,
                DurationInSemesters = model.DurationInSemesters.Value,
                DepartmentId = model.DepartmentId.Value
            };

            _context.AcademicPrograms.Add(academicProgram);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Academic program added successfully",
                academicProgram.Id
            });
        }

        // PUT: api/AcademicProgram/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAcademicProgram(int id, [FromBody] AcademicProgramDTO model)
        {
            var academicProgram = await _context.AcademicPrograms
                .FirstOrDefaultAsync(p => p.Id == id);

            if (academicProgram == null)
            {
                return NotFound(new { message = "Academic program not found" });
            }

            if (string.IsNullOrWhiteSpace(model.ProgramName))
            {
                return BadRequest(new { message = "Program name is required" });
            }

            if (string.IsNullOrWhiteSpace(model.ProgramCode))
            {
                return BadRequest(new { message = "Program code is required" });
            }

            if (!model.DurationInSemesters.HasValue || model.DurationInSemesters <= 0)
            {
                return BadRequest(new { message = "Duration in semesters must be a positive number" });
            }

            if (!model.DepartmentId.HasValue)
            {
                return BadRequest(new { message = "Department is required" });
            }

            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == model.DepartmentId.Value);

            if (!departmentExists)
            {
                return BadRequest(new { message = "Department does not exist" });
            }

            var trimmedCode = model.ProgramCode.Trim();
            var duplicateCode = await _context.AcademicPrograms
                .AnyAsync(p => p.ProgramCode != null && p.ProgramCode.ToLower() == trimmedCode.ToLower() && p.Id != id);

            if (duplicateCode)
            {
                return BadRequest(new { message = "Program code already exists" });
            }

            academicProgram.ProgramName = model.ProgramName.Trim();
            academicProgram.ProgramCode = trimmedCode;
            academicProgram.DurationInSemesters = model.DurationInSemesters.Value;
            academicProgram.DepartmentId = model.DepartmentId.Value;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Academic program updated successfully" });
        }

        // DELETE: api/AcademicProgram/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAcademicProgram(int id)
        {
            var academicProgram = await _context.AcademicPrograms
                .FirstOrDefaultAsync(p => p.Id == id);

            if (academicProgram == null)
            {
                return NotFound(new { message = "Academic program not found" });
            }

            // Check whether program is assigned to any students
            var isReferencedByStudents = await _context.Students
                .AnyAsync(s => s.AcademicProgramId == id);

            if (isReferencedByStudents)
            {
                return BadRequest(new { message = "Cannot delete academic program because it is assigned to one or more students." });
            }

            // Check whether program has associated semesters
            var isReferencedBySemesters = await _context.Semesters
                .AnyAsync(s => s.AcademicProgramId == id);

            if (isReferencedBySemesters)
            {
                return BadRequest(new { message = "Cannot delete academic program because it has associated semesters." });
            }

            try
            {
                _context.AcademicPrograms.Remove(academicProgram);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Academic program deleted successfully" });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Cannot delete academic program because it is referenced by other records." });
            }
        }
    }
}


