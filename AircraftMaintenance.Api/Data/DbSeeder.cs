using Microsoft.AspNetCore.Identity;

namespace AircraftMaintenance.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            // 1. Ensure the database is created and migrations are applied
           
            // 2. Seed Roles
            string[] roles = { "Mechanic", "HR", "Supervisor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            // 3. Seed Users (Email format for Identity)
            await CreateUserIfNotExists(userManager, "mechanic@aircraft.com", "Mechanic@123", "Mechanic");
            await CreateUserIfNotExists(userManager, "hr@aircraft.com", "Hr@123", "HR");
            await CreateUserIfNotExists(userManager, "supervisor@aircraft.com", "Super@123", "Supervisor");
        }

        private static async Task CreateUserIfNotExists(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
    }
