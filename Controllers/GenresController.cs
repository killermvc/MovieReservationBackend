using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using MovieReservation.Models;

namespace MovieReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController(AppDbContext _db) : ControllerBase
{

	[HttpGet]
	[Authorize]
	public IActionResult GetAll()
	{
		var data = _db.Genres.ToList();
		return Ok(data);
	}

	[HttpGet("{id}")]
	[Authorize]
	public async Task<IActionResult> GetById(int id)
	{
		var data = await _db.Genres.FindAsync(id);
		if (data == null)
		{
			return NotFound();
		}

		return Ok(data);
	}

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create(string name)
	{
		var genre = new Genre { Name = name };
		await _db.Genres.AddAsync(genre);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = genre.Id }, genre);
	}

	[HttpPut("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Update(int id, string name)
	{
		var data = await _db.Genres.FindAsync(id);
		if (data == null)
		{
			return NotFound();
		}

		data.Name = name;
		await _db.SaveChangesAsync();
		return Ok(data);
	}

	[HttpDelete("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id)
	{
		var data = await _db.Genres.FindAsync(id);
		if (data == null)
		{
			return NotFound();
		}

		_db.Genres.Remove(data);
		await _db.SaveChangesAsync();
		return NoContent();
	}

}