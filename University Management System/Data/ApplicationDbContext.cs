using Microsoft.EntityFrameworkCore;
using University_Management_System.Models.MainModels;
namespace University_Management_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {

        }

        public DbSet <Teacher> Teachers { get; set; }
        public DbSet <Course> Courses { get; set; }
        public DbSet<Assignment> Assignment { get; set; }
        public DbSet<TeacherCourse> TeacherCourse { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Setting> Settings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TeacherCourse>()
                .HasKey(tc => new { tc.TeacherId, tc.CourseId });




            //TeacherCourse Fluient APIs

            modelBuilder.Entity<TeacherCourse>()
                .HasOne(tc => tc.Teacher)
                .WithMany()
                .HasForeignKey(tc => tc.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TeacherCourse>()
                .HasOne(tr => tr.Course)
                .WithMany()
                .HasForeignKey(tr => tr.CourseId)
                .OnDelete(DeleteBehavior.NoAction);


            //Assignment Fluient APIs

            modelBuilder.Entity<Assignment>()
                .HasOne(tc => tc.Course)
                .WithMany()
                .HasForeignKey(tc => tc.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Assignment>()
                .HasOne(tr => tr.AssignedTeacher)
                .WithMany()
                .HasForeignKey(tr => tr.AssignedTeacherId)
                .OnDelete(DeleteBehavior.NoAction);
                
                }
    }
    
    
}
