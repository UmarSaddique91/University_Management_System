using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.StudentManagement
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize (Roles = SystemRoles.superAdmin)]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentController(ApplicationDbContext context,UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        //Post API for User

        [HttpPost]

        public async Task <IActionResult> AddStudent([FromBody] AddStudentDTO model)
        {
            var student = new Student
            {
                UserId = model.UserId,
                StudentName = model.StudentName,
                RollNumber = model.RollNumber,
                DepartmentId = model.DepartmentId,
                Program = model.Program,
                Semester = model.Semester,
                Status = model.Status
            };

            //Check the duplication of Student

            var exisitngStudent = _context.Students.FirstOrDefault(s => s.RollNumber == model.RollNumber);


            if (exisitngStudent != null)
            {
                return BadRequest(new { message = "Roll number already exists" });
            }

            //Check whether the Department Exist or not

            if(model.DepartmentId != null)
            {
                var departmentExist = _context.Departments.Any(d => d.Id == model.DepartmentId);

                if (!departmentExist)
                {
                    return BadRequest(new { message = "Department Not Find" });
                }
            }

            //Check User Id whther user is registered or not

            var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return BadRequest(new { message = "User does not registred" });
                }

            //Check whether it has student Role or not

            var isStudent = await _userManager.IsInRoleAsync(user,SystemRoles.Student);

            if (!isStudent)
            {
                return BadRequest(new { message = "User Does not have Student Role" });
            }
                        
            _context.Students.Add(student);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Student Added Successfully",
                student.Id
            });
        }



        //Get Api

        [HttpGet]
        public async Task <IActionResult> GetStudents()
        {
            var students =await  _context.Students
                .Select(student => new
                {
                    student.Id,
                    student.UserId,
                    student.StudentName,
                    student.RollNumber,
                    student.DepartmentId,
                    DepartmentName = student.Department != null
            ? student.Department.DepartmentName
            : null,
                    student.Program,
                    student.Semester,
                    student.Status
                })
                .ToListAsync();

            return Ok(students);
        }


        //Get Student by Id

        [HttpGet("{id}")]
        public IActionResult GetStudentById(int id)
        {
            var student = _context.Students
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            return Ok(student);
        }


        //Update the student Detail

        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id, [FromBody] AddStudentDTO model)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            student.StudentName = model.StudentName;
            student.RollNumber = model.RollNumber;
            student.DepartmentId = model.DepartmentId;
            student.Program = model.Program;
            student.Semester = model.Semester;
            student.Status = model.Status;


            //Check whether the Roll Number given by admin is already assigned to any student or not 
            var duplicateRollNumber = _context.Students
    .Any(s => s.RollNumber == model.RollNumber && s.Id != id);

            if (duplicateRollNumber)
            {
                return BadRequest(new { message = "Roll number already exists" });
            }

            //Check whether the department Id given byt the user exist in Department table or not?

            if (model.DepartmentId != null)
            {
                var departmentExists = _context.Departments
                    .Any(d => d.Id == model.DepartmentId);

                if (!departmentExists)
                {
                    return BadRequest(new { message = "Department does not exist" });
                }
            }

            _context.SaveChanges();

            return Ok(new { message = "Student updated successfully" });
        }


        //Delete Student API

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            var student = _context.Students
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            _context.Students.Remove(student);
            _context.SaveChanges();

            return Ok(new { message = "Student deleted successfully" });
        }
    }
}
