using University_Management_System.Constants;
using Microsoft.AspNetCore.Identity;


namespace University_Management_System.Data.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                SystemRoles.superAdmin,
                SystemRoles.Admin,
                SystemRoles.Registrar,
                SystemRoles.DepartmentAdmin,
                SystemRoles.Teacher,
                SystemRoles.Student,
                SystemRoles.Accountant,
                SystemRoles.ExamController,
                SystemRoles.Librarian,
                SystemRoles.HR
            };

            foreach(var role in roles)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
