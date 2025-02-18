using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MovieReservation.Models;

public class SeedData
{
	public static async Task InitializeRoles(RoleManager<IdentityRole> roleManager)
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
	}

	public static async Task InitializeAdmins(UserManager<User> userManager, IConfiguration configuration)
	{
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

	public static async Task InitializeCinema(AppDbContext db)
	{
		// Seed cinema
		var cinema = await db.Cinemas
			.Where(c => c.Name == "Cinema 1" && c.Location == "123 Main St, City")
			.FirstOrDefaultAsync();
		if (cinema == null)
		{
			cinema = new Cinema
			{
				Name = "Cinema 1",
				Location = "123 Main St, City"
			};
			db.Cinemas.Add(cinema);
		}

		var hall1 = await db.CinemaHalls
			.Where(h => h.Name == "Hall 1" && h.Cinema == cinema)
			.FirstOrDefaultAsync();
		if (hall1 == null)
		{
			hall1 = new CinemaHall
			{
				Name = "Hall 1",
				Capacity = 50,
				Cinema = cinema
			};
			db.CinemaHalls.Add(hall1);
		}

		var hall2 = await db.CinemaHalls
			.Where(h => h.Name == "Hall 2" && h.Cinema == cinema)
			.FirstOrDefaultAsync();
		if (hall2 == null)
		{
			hall2 = new CinemaHall
			{
				Name = "Hall 2",
				Capacity = 100,
				Cinema = cinema
			};
			db.CinemaHalls.Add(hall2);
		}

		await db.SaveChangesAsync();
	}

    public static async Task InitializeAsync(AppDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        await InitializeRoles(roleManager);
        await InitializeAdmins(userManager, configuration);
		await InitializeCinema(db);
    }
}