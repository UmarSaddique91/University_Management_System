using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using University_Management_System.Models.MainModels;
using Microsoft.AspNetCore.Identity;
namespace University_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {

        }

        public DbSet <Teacher> Teachers { get; set; }
        public DbSet <Course> Courses { get; set; }
        
       

        public DbSet<Department> Departments { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<AcademicProgram> AcademicPrograms { get; set; }

        public DbSet<AcademicSession> AcademicSessions { get; set; }

        public DbSet<Semester> Semesters { get; set; }
        public DbSet <CourseOffering> CourseOfferings { get; set; }

        public DbSet <Enrollment> Enrollments { get; set; }

        public DbSet<Attendance> Attendance { get; set; }

        public DbSet <Exam> Exams { get; set; }

        public DbSet<Grade> Grades { get; set; }


        
    }
    
    
}
