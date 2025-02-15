using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MovieReservation.Controllers;

using Microsoft.AspNetCore.Authorization;
using MovieReservation.Models;

[ApiController]
[Route("api/[controller]")]
public class RolesController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager) : ControllerBase
{
	private readonly UserManager<User> _userManager = userManager;
	private readonly RoleManager<IdentityRole> _roleManager = roleManager;

	[Authorize(Roles = "Admin")]
	[HttpPost("create-role")]
	public async Task<IActionResult> CreateRole([FromBody] string roleName)
	{
		var roleExists = await _roleManager.RoleExistsAsync(roleName);
		if (roleExists)
			return BadRequest("Role already exists");

		var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
		return result.Succeeded ? Ok("Role created successfully") : BadRequest(result.Errors);
	}

	[Authorize(Roles = "Admin")]
	[HttpPost("assign-role")]
	public async Task<IActionResult> AssignRole([FromBody] RoleAssignmentModel model)
	{
		var user = await _userManager.FindByEmailAsync(model.Email);
		if (user == null)
			return NotFound("User not found");

		var result = await _userManager.AddToRoleAsync(user, model.Role);
		return result.Succeeded ? Ok("Role assigned successfully") : BadRequest(result.Errors);
	}

}

public class RoleAssignmentModel
{
	public string Email { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
}