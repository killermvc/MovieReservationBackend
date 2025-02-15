using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MovieReservation.Controllers;

using MovieReservation.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserManager<User>  userManager, SignInManager<User> signInManager) : ControllerBase
{

	private readonly UserManager<User> _userManager = userManager;
	private readonly SignInManager<User> _signInManager = signInManager;

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterModel model)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var user = new User { UserName = model.Username, Email = model.Email };
		var result = await _userManager.CreateAsync(user, model.Password);

		if (!result.Succeeded)
			return BadRequest(result.Errors);

		return Ok(new { message = "User registered successfully" });
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginModel model)
	{
		var user = await _userManager.FindByEmailAsync(model.Email);
		if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
			return Unauthorized(new { message = "Invalid credentials" });

		var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
		if (!result.Succeeded)
			return Unauthorized(new { message = "Invalid credentials" });

		return Ok(new { message = "Login successful" });
	}

	[HttpPost("logout")]
	public async Task<IActionResult> Logout()
	{
		await _signInManager.SignOutAsync();
		return Ok(new { message = "Logged out successfully" });
	}

	[HttpGet("me")]
	public IActionResult GetCurrentUser()
	{
		if (User.Identity is not null && !User.Identity.IsAuthenticated)
			return Unauthorized(new { message = "Not logged in" });

		return Ok(new { username = User.Identity!.Name });
	}

}

public class RegisterModel
{
	public string Username { get; set; } = String.Empty;
	public string Email { get; set; } = String.Empty;
	public string Password { get; set; } = String.Empty;
}

public class LoginModel
{
	public string Email { get; set; } = String.Empty;
	public string Password { get; set; } = String.Empty;
}