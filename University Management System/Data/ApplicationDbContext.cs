using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using University_Management_System.Models.MainModels;
using Microsoft.AspNetCore.Identity;
namespace University_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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


            // Seeder for Admin
            // NOTE: Avoid using runtime-generated values (e.g. PasswordHasher.HashPassword, Guid.NewGuid(), DateTime.Now)
            // in HasData because EF Core records the model snapshot. Dynamic values change every build and
            // cause the "model changes each time it is built" warning. Use a fixed, hard-coded PasswordHash
            // here (or seed users at application startup using UserManager).

            var admin = new ApplicationUser
            {
                Id = "1",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@ums.com",
                NormalizedEmail = "ADMIN@UMS.COM",
                EmailConfirmed = true,
                FullName = "System Admin",
                SecurityStamp = "ABC123",
                // Keep ConcurrencyStamp static to avoid model snapshot changes
                ConcurrencyStamp = "e338f354-114b-486b-b727-b4c079503180",
                // Use the static password hash that matches the existing migration snapshot so the model
                // does not change on every build. If you prefer to create the user with a real password,
                // create it at runtime using UserManager.CreateAsync instead of HasData.
                PasswordHash = "AQAAAAIAAYagAAAAEHZ06/YrybQ5dLG2BX4B1Y4RHMsaGGeEakANkxxbVi5w+hrosL7qBRw2Z8e1wyOuiA=="
            };

            modelBuilder.Entity<ApplicationUser>().HasData(admin);
                }
    }
    
    
}
