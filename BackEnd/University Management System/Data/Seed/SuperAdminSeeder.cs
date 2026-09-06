using Microsoft.AspNetCore.Identity;
using University_Management_System.Constants;
namespace University_Management_System.Data.Seed
{
    public static class SuperAdminSeeder
    {

        public static async Task SuperAdminSeederAsync(UserManager<IdentityUser> userManager,
            IConfiguration configuration)
        {
            string? email = configuration["SuperAdmin:Email"];
            string? password = configuration["SuperAdmin:Password"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return;
            }


            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var superAdmin = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(superAdmin, password);



                //For Assign Role from System Roles
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        superAdmin,
                        SystemRoles.superAdmin
                        );
                }

                
            }

            

        }
    }
}
