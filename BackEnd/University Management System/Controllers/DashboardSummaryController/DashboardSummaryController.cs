using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Management_System.Data;
using University_Management_System.DTOs;

namespace University_Management_System.Controllers.DashboardSummaryController
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardSummaryController : ControllerBase
    {
        
        private readonly ApplicationDbContext _context;
        public  DashboardSummaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]

        public async Task<ActionResult<DashboardSummaryDTO>> GetSummary()
        {

            var now = DateTime.UtcNow;

            var startOfMonth = new DateTime(

                now.Year,
                now.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc
                );

            var startOfNextMonth = startOfMonth.AddMonths(1);

            var startOfPreviousMonth = startOfMonth.AddMonths(-1);


            //Student Last month and percentage Logic

            var studentOfPreviousMonth = await _context.Students.CountAsync(

                st => st.CreatedAt < startOfMonth &&
                st.CreatedAt >= startOfPreviousMonth
                );

            var StudentOfCurrentMonth = await _context.Students.CountAsync(
                    st => st.CreatedAt >= startOfMonth &&
                     st.CreatedAt < startOfNextMonth
                    );


            //Calculating the Student Growth

            double studentGrowthPercentage = 0;

            if (studentOfPreviousMonth > 0)
            {
                studentGrowthPercentage =
                    ((double)(StudentOfCurrentMonth - studentOfPreviousMonth)
                    / studentOfPreviousMonth) * 100;

            }



            //Teacher Last month and percentage Logic

            var TeacherOfThisMonth = await _context.Teachers.CountAsync(
                t => t.CreatedAt >= startOfMonth &&
                t.CreatedAt < startOfNextMonth
                );

            var TeacherOfPreviousMonth = await _context.Teachers.CountAsync(

                t => t.CreatedAt >= startOfPreviousMonth &&
                t.CreatedAt < startOfMonth

                );

            //Calculating the Teacher Growth

            double teacherGrowthPercentage = 0;

            if (TeacherOfPreviousMonth > 0)
            {
                teacherGrowthPercentage =
                    ((double)(TeacherOfThisMonth - TeacherOfPreviousMonth) /
                    TeacherOfPreviousMonth) * 100;
                    
                    
                    
                    
            }


            //Enrollment Last month and percentage Logic

            var enrollmentThisMonth = await _context.Enrollments.CountAsync(
                en=> en.CreatedAt >= startOfMonth&&
                en.CreatedAt< startOfNextMonth&&
                en.Status=="active"
                );

            var enrollmentLastMonth = await _context.Enrollments.CountAsync(
                en => en.CreatedAt >= startOfPreviousMonth &&
                en.CreatedAt < startOfMonth &&
                en.Status == "active"
                );



            //Active Enrollment Percentage Logic

            double enrollmentGrowthPercentage = 0;

            if (enrollmentLastMonth > 0)
            {
                enrollmentGrowthPercentage = ((double)(enrollmentThisMonth - enrollmentLastMonth) /

                    enrollmentLastMonth) * 100;
            }


            var summary = new DashboardSummaryDTO
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalTeachers = await _context.Teachers.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync(),



                //Only select the enrollments that are active

                ActiveEnrollments = await _context.Enrollments.CountAsync(en => en.Status == "active"),

                
                //Stuents Logic

               StudentThisMonth = StudentOfCurrentMonth,
                StudentGrowthPercentage = studentGrowthPercentage,


                //TeacherLogic

                TeacherThisMonth= TeacherOfThisMonth,
                TeacherGrowthPercentage= teacherGrowthPercentage,


                //Enrollment Logic


                EnrollmentThisMonth=enrollmentThisMonth,
                EnrollmentGrowthPercentage = enrollmentGrowthPercentage





            };

            return Ok(summary);
            
        }
    }
}
