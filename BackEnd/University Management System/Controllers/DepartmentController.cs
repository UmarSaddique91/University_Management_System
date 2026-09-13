using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Post Api

        [HttpPost]

        public IActionResult AddDepartment([FromBody] Department department)
        {
            if (department == null)
            {
                return BadRequest("Department Data is Invalid");
            }

            _context.Departments.Add(department);
            _context.SaveChanges();
            return Ok("Department Added Successfully"); 
        }

        //GET API

        [HttpGet]

        public IActionResult GetAllDepartment()
        {
            var departments = _context.Departments
        .Select(d => new
        {
            d.Id,
            d.DepartmentName
        })
        .ToList();

            return Ok(departments);
        }

        //PutAPI

        [HttpPut("{Id}")]

        public IActionResult UpdateDepartment(int Id, [FromBody] Department department)
        {
            var ExistingDepartment = _context.Departments.Find(Id);

            if (ExistingDepartment == null)
            {
                return NotFound("Deapertment is not found");
            }

            var updatedDepartment=ExistingDepartment.DepartmentName = department.DepartmentName;
            _context.SaveChanges();
            return Ok(updatedDepartment);
        }

        //DeleteAPI

        [HttpDelete("{Id}")]

        public IActionResult DepartmentDelete(int Id)
        {
            var departmentToDelete = _context.Departments.Find(Id);

            if (departmentToDelete == null)
            {
                return NotFound("Department Not Found");
            }

            var deletedDepartment = _context.Departments.Remove(departmentToDelete);
            _context.SaveChanges();
            return Ok("Department Deleted Successfully");
        }
    
    }
}
