using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        public readonly ApplicationDbContext _context;
        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }


        //Crud Operation

        //PostApi
        [HttpPost]

        public IActionResult AddTeacher([FromBody] Teacher teacher)
        {
            if (teacher == null)
            {
                return BadRequest("Teacher Data is Invalid");
            }

            _context.Teachers.Add(teacher);
            _context.SaveChanges();

            return Ok("Teacher Added Successfully");
        }


        //GetApi

        [HttpGet]

        public IActionResult GetTeachers(string?name, string?teacherId, string?department , string?status)
        {
            var query = _context.Teachers.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            
                query = query.Where(tr => tr.TeacherName.Contains(name));

            if (!string.IsNullOrEmpty(teacherId))

                query = query.Where(tr => tr.TeacherId.Contains(teacherId));

            if (!string.IsNullOrEmpty(department))
                query = query.Where(tr => tr.DepartmentName.Contains(department));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(tr => tr.Status.Contains(status));

            var result = query.ToList();
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

            if (!string.IsNullOrEmpty(teacher.TeacherName))
                existingTeacher.TeacherName = teacher.TeacherName;
            if (!string.IsNullOrEmpty(teacher.DepartmentName))
                existingTeacher.DepartmentName = teacher.DepartmentName;

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

        public IActionResult DeleteTeacher(int Id, [FromBody] Teacher teacher)
        {
            var TeacherToDelete = _context.Teachers.Find(Id);

            if(TeacherToDelete == null)
            {
                return NotFound("Teacher Not Found");
            }

            _context.Teachers.Remove(TeacherToDelete);
            _context.SaveChanges();
            return Ok("Teacher Updated Successfully");
        }

        
    }
}
