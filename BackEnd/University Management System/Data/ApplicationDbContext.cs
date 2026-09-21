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
        
        public DbSet<RegistrationInvitation> RegistrationInvitations { get; set; }
       

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

        public DbSet<AcademicProgramCourse> AcademicProgramCourses { get; set; }

        public DbSet<AdminProfile> AdminProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CourseOffering>()
                .Property(co => co.Status)
                .HasDefaultValue("Active");

            builder.Entity<Semester>()
                .Property(s => s.Status)
                .HasDefaultValue("Active");

            builder.Entity<Student>()
                .HasOne(s => s.CurrentSemester)
                .WithMany()
                .HasForeignKey(s => s.CurrentSemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseOfferingId })
                .IsUnique();

            builder.Entity<Enrollment>()
                .Property(e => e.Status)
                .HasDefaultValue("Active");
        }
    }
}
