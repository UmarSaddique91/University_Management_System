using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Constants;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;
using Microsoft.AspNetCore.Identity;
using University_Management_System.DTOs;

namespace University_Management_System.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public TeacherController(ApplicationDbContext context,UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager=userManager;
        }


        //Crud Operation

        //PostApi
        [HttpPost]

        public IActionResult AddTeacher([FromBody] AddTeacherDTO teacher)
        {
            if (teacher == null)
            {
                return BadRequest("Teacher Data is Invalid");
            }

            var newTeacher = new Teacher
            {
                TeacherId = teacher.TeacherId,
                Email = teacher.Email,
                TeacherName = teacher.TeacherName,
                DepartmentId = teacher.DepartmentId,
                Specialization = teacher.Specialization,
                Status = teacher.Status
            };

            _context.Teachers.Add(newTeacher);
            _context.SaveChanges();

            return Ok("Teacher Added Successfully");
        }


        //GetApi

        [Authorize(Roles = SystemRoles.superAdmin)]

        [HttpGet]

        
        public IActionResult GetTeachers(string?name, string?teacherId, int?DepartmentId , string?status)
        {
            var query = _context.Teachers.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            
                query = query.Where(tr => tr.TeacherName.Contains(name));

            if (!string.IsNullOrEmpty(teacherId))

                query = query.Where(tr => tr.TeacherId.Contains(teacherId));

            if (DepartmentId.HasValue)
                query = query.Where(tr => tr.DepartmentId== DepartmentId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(tr => tr.Status.Contains(status));

            var result = query.Select(t => new AddTeacherDTO
            {
                Id = t.Id,
                TeacherId = t.TeacherId,
                UserId = t.UserId,
                Email = t.Email,
                TeacherName = t.TeacherName,

                DepartmentId = t.DepartmentId,
                DepartmentName = t.Department != null
        ? t.Department.DepartmentName
        : null,

                Specialization = t.Specialization,
                Status = t.Status
            }).ToList();
            return Ok(result);
        }

        
        //UpdateApi

        [HttpPut("{Id}")]

        public IActionResult UpdateTeacher(int Id, [FromBody] Teacher teacher)
        {
            var existingTeacher = _context.Teachers.Find(Id);

            if (existingTeacher == null)
            {
                return NotFound("Teacher Not Found");
            }

            if (!string.IsNullOrEmpty(teacher.TeacherId))
            existingTeacher.TeacherId = teacher.TeacherId;

            if (!string.IsNullOrEmpty(teacher.Email))
                existingTeacher.Email = teacher.Email;

            if (!string.IsNullOrEmpty(teacher.TeacherName))
                existingTeacher.TeacherName = teacher.TeacherName;

            if (teacher.DepartmentId.HasValue)
                existingTeacher.DepartmentId = teacher.DepartmentId;

            if (!string.IsNullOrEmpty(teacher.Specialization))
                existingTeacher.Specialization = teacher.Specialization;

            if (!string.IsNullOrEmpty(teacher.Contact))
                existingTeacher.Contact = teacher.Contact;

            if (!string.IsNullOrEmpty(teacher.Status))
                existingTeacher.Status = teacher.Status;

            

            _context.SaveChanges();
            return Ok("Teacher Updated Successfully");
        }

        //DeleteApi

        [HttpDelete("{Id}")]

        public async Task<IActionResult> DeleteTeacher(int Id)
        {
            var TeacherToDelete = _context.Teachers.Find(Id);

            if(TeacherToDelete == null)
            {
                return NotFound("Teacher Not Found");
            }



            var user = await _userManager.FindByIdAsync(TeacherToDelete.UserId!);


            if (user != null)
            {
                var userResult = await _userManager.DeleteAsync(user);

                if (!userResult.Succeeded)
                {
                    return BadRequest(userResult.Errors);
                }
            }

            _context.Teachers.Remove(TeacherToDelete);
            _context.SaveChanges();
            return Ok("Teacher Updated Successfully");
        }

        
    }
}
