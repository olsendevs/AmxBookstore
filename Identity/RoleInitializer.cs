using AmxBookstore.Domain.Entities.Users.Enum;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;


namespace AmxBookstore.Infrastructure.Identity
{
    public static class RoleInitializer
    {
        public static async Task InitializeAsync(RoleManager<IdentityRole<Guid>> roleManager, UserManager<User> userManager)
        {
            string[] roleNames = { "Admin", "Seller", "Client" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
                }
            }

            var adminUser = new User("adminstrator", "admin@example.com", "Admin123!", UserRoles.Admin, Guid.NewGuid());

            var user = await userManager.FindByEmailAsync(adminUser.Email);

            if (user == null)
            {
                var createPowerUser = await userManager.CreateAsync(adminUser, "Admin123!");
  
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
