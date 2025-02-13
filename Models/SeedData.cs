using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace MovieReservation.Models;

public class SeedData
{
    public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        // Seed roles
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var userRole = await roleManager.FindByNameAsync("User");
        if (userRole == null)
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Seed admin account
        var adminUser = await userManager.FindByNameAsync("admin");
		if (adminUser == null)
		{
			adminUser = new User
			{
				UserName = "admin",
				Email = "admin@example.com"
			};

			var adminPassword = configuration["AdminPassword"];
			if (string.IsNullOrEmpty(adminPassword))
			{
				throw new InvalidOperationException("Admin password is not configured.");
			}

			var result = await userManager.CreateAsync(adminUser, adminPassword);
			if (!result.Succeeded)
			{
				throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
			}
		}

		await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}