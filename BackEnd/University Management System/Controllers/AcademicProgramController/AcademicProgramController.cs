using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;
using University_Management_System.DTOs;
using Microsoft.AspNetCore.Authorization;
using University_Management_System.Constants;


namespace University_Management_System.Controllers.AcademicProgramController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles= SystemRoles.superAdmin)]
    public class AcademicProgramController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public AcademicProgramController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Post API for AcedemicPrograms

        [HttpPost]
        public IActionResult AddAcademicProgram([FromBody] AcademicProgramDTO model)
        {

            //Check if the department is Exist or Not
            if (model.DepartmentId != null)
            {
                var departmentExists = _context.Departments
                    .Any(d => d.Id == model.DepartmentId);

                if (!departmentExists)
                {
                    return BadRequest(new { message = "Department does not exist" });
                }
            }

            //Check if the program is already added

            var duplicateProgram = _context.AcademicPrograms
                .Any(p => p.ProgramCode == model.ProgramCode);

            if (duplicateProgram)
            {
                return BadRequest(new { message = "Program code already exists" });
            }

            var academicProgram = new AcademicProgram
            {
                ProgramName = model.ProgramName,
                ProgramCode = model.ProgramCode,
                DurationInSemesters = model.DurationInSemesters,
                DepartmentId = model.DepartmentId
            };

            _context.AcademicPrograms.Add(academicProgram);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Academic program added successfully",
                academicProgram.Id
            });
        }


        //Get Api for Programs

        [HttpGet]
        public IActionResult GetAcademicPrograms()
        {
            var programs = _context.AcademicPrograms
                .Select(program => new
                {
                    program.Id,
                    program.ProgramName,
                    program.ProgramCode,
                    program.DurationInSemesters,
                    program.DepartmentId,
                    DepartmentName = program.Department != null
                        ? program.Department.DepartmentName
                        : null
                })
                .ToList();

            return Ok(programs);
        }


        //PostApi for AcademicPrograms

        [HttpPut("{id}")]
        public IActionResult UpdateAcademicProgram(int id,[FromBody] AcademicProgramDTO model)
        {

            //Find the Id of the program that we need to update
            var academicProgram = _context.AcademicPrograms
                .FirstOrDefault(p => p.Id == id);

            if (academicProgram == null)
            {
                return NotFound(new { message = "Academic program not found" });
            }

            //Find the department

            if (model.DepartmentId != null)
            {
                var departmentExists = _context.Departments
                    .Any(d => d.Id == model.DepartmentId);

                if (!departmentExists)
                {
                    return BadRequest(new { message = "Department does not exist" });
                }
            }

            //Make sure the there is no already store program code same as given code
            var duplicateCode = _context.AcademicPrograms
                .Any(p => p.ProgramCode == model.ProgramCode && p.Id != id);

            if (duplicateCode)
            {
                return BadRequest(new { message = "Program code already exists" });
            }

            academicProgram.ProgramName = model.ProgramName;
            academicProgram.ProgramCode = model.ProgramCode;
            academicProgram.DurationInSemesters = model.DurationInSemesters;
            academicProgram.DepartmentId = model.DepartmentId;

            _context.SaveChanges();

            return Ok(new { message = "Academic program updated successfully" });
        }

        //Delete Api for Academic Programs

        [HttpDelete("{id}")]
        public IActionResult DeleteAcademicProgram(int id)
        {
            var academicProgram = _context.AcademicPrograms
                .FirstOrDefault(p => p.Id == id);

            if (academicProgram == null)
            {
                return NotFound(new { message = "Academic program not found" });
            }

            _context.AcademicPrograms.Remove(academicProgram);
            _context.SaveChanges();

            return Ok(new { message = "Academic program deleted successfully" });
        }
    }
}
