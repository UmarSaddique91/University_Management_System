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


        //Get for Cards

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
                en => en.CreatedAt >= startOfMonth &&
                en.CreatedAt < startOfNextMonth &&
                en.Status != null && (en.Status.ToLower() == "active" || en.Status.ToLower() == "enrolled") &&
                en.CourseOffering != null && en.CourseOffering.Status == "Active"
                );

            var enrollmentLastMonth = await _context.Enrollments.CountAsync(
                en => en.CreatedAt >= startOfPreviousMonth &&
                en.CreatedAt < startOfMonth &&
                en.Status != null && (en.Status.ToLower() == "active" || en.Status.ToLower() == "enrolled") &&
                en.CourseOffering != null && en.CourseOffering.Status == "Active"
                );

            //Active Enrollment Percentage Logic

            double enrollmentGrowthPercentage = 0;

            if (enrollmentLastMonth > 0)
            {
                enrollmentGrowthPercentage = Math.Round(((double)(enrollmentThisMonth - enrollmentLastMonth) /
                    enrollmentLastMonth) * 100, 1);
            }
            else if (enrollmentThisMonth > 0)
            {
                enrollmentGrowthPercentage = 100.0;
            }

            var summary = new DashboardSummaryDTO
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalTeachers = await _context.Teachers.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync(),

                // Only select the enrollments that are active/enrolled AND belonging to an Active CourseOffering
                ActiveEnrollments = await _context.Enrollments.CountAsync(en =>
                    en.Status != null && (en.Status.ToLower() == "active" || en.Status.ToLower() == "enrolled") &&
                    en.CourseOffering != null && en.CourseOffering.Status == "Active"
                ),

                //Students Logic
                StudentThisMonth = StudentOfCurrentMonth,
                StudentGrowthPercentage = Math.Round(studentGrowthPercentage, 1),

                //Teacher Logic
                TeacherThisMonth = TeacherOfThisMonth,
                TeacherGrowthPercentage = Math.Round(teacherGrowthPercentage, 1),

                //Enrollment Logic
                EnrollmentThisMonth = enrollmentThisMonth,
                EnrollmentGrowthPercentage = enrollmentGrowthPercentage
            };

            return Ok(summary);
            
        }



        //Get for Trend

        [HttpGet("trend")]

        public async Task<IActionResult> GetEnrollmendTrend([FromQuery] string period= "6 months")
        {
            var now = DateTime.UtcNow;

            var startDate = period switch
            {
                "1 Year" => now.AddYears(-1),
                "6 Months" => now.AddMonths(-6),
                "3 Months" => now.AddMonths(-3),
                "1 Month" => now.AddMonths(-1),
                "1 Week" => now.AddDays(-7),

                //if period does not match with any defined option

                _ => now.AddMonths(-6)
            };


            var enrollmetns = await _context.Enrollments.Where(
                en => en.Status != null && (en.Status.ToLower() == "active" || en.Status.ToLower() == "enrolled") &&
                en.CourseOffering != null && en.CourseOffering.Status == "Active" &&
                en.CreatedAt >= startDate &&
                en.CreatedAt <= now
                ).CountAsync();


            return Ok(period);
        }
    }
}
