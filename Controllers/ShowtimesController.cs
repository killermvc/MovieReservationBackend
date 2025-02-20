using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MovieReservation.Models;

namespace MovieReservation.Controllers;

[ApiController]
[Route("api/showtimes")]
public class ShowtimesController(AppDbContext _db) : ControllerBase
{
	[HttpGet]
	public IActionResult GetAll()
	{
		var showtimes = _db.Showtimes.ToList();
		return Ok(showtimes.Select(s => new ShowtimeDTO(s)));
	}

	[HttpGet("movie/{movieId}")]
	public IActionResult GetAllForMovie(int movieId)
	{
		var showtimes = _db.Showtimes.Where(s => s.MovieId == movieId).ToList();
		return Ok(showtimes.Select(s => new ShowtimeDTO(s)));
	}

	[HttpGet("day/{date}")]
	public IActionResult GetAllForDay(DateTime date)
	{
		var showtimes = _db.Showtimes.Where(s => s.StartTime.Date == date.Date).ToList();
		return Ok(showtimes.Select(s => new ShowtimeDTO(s)));
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(int id)
	{
		var showtime = await _db.Showtimes.FindAsync(id);
		if (showtime == null)
		{
			return NotFound();
		}
		return Ok(new ShowtimeDTO(showtime));
	}

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] ShowtimeCreateDTO createDTO)
	{
		if (!await _db.CinemaHalls.AnyAsync(c => c.Id == createDTO.CinemaHallId))
		{
			return BadRequest("CinemaHallId is not valid");
		}

		if (!await _db.Movies.AnyAsync(m => m.Id == createDTO.MovieId))
		{
			return BadRequest("MovieId is not valid");
		}

		if (createDTO.StartTime < DateTime.Now)
		{
			return BadRequest("StartTime should be in the future");
		}
		var showtime = createDTO.ToShowtime();
		await _db.Showtimes.AddAsync(showtime);
		await _db.SaveChangesAsync();

		var cinemaHall = await _db.CinemaHalls.FindAsync(showtime.CinemaHallId);
		showtime.CinemaHall = cinemaHall;
		showtime.InitializeSeats();

		return CreatedAtAction(nameof(GetById), new { id = showtime.Id }, new ShowtimeDTO(showtime));
	}

	[HttpPut("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Update(int id, [FromBody] ShowtimeUpdateDTO updateDTO)
	{
		var data = await _db.Showtimes.FindAsync(id);
		if (data == null)
		{
			return NotFound();
		}

		data.StartTime = updateDTO.StartTime;
		data.CinemaHallId = updateDTO.CinemaHallId;
		data.MovieId = updateDTO.MovieId;

		await _db.SaveChangesAsync();
		return Ok(new ShowtimeDTO(data));
	}

	[HttpDelete("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id)
	{
		var showtime = await _db.Showtimes.FindAsync(id);
		if (showtime == null)
		{
			return NotFound();
		}

		_db.Showtimes.Remove(showtime);
		await _db.SaveChangesAsync();
		return Ok();
	}
}

public class ShowtimeDTO(Showtime showtime)
{
	public int Id { get; set; } = showtime.Id;
	public DateTime StartTime { get; set; } = showtime.StartTime;
	public int CinemaHallId { get; set; } = showtime.CinemaHallId;
	public int MovieId { get; set; } = showtime.MovieId;

}

public class ShowtimeUpdateDTO
{
	public DateTime StartTime { get; set; }
	public int CinemaHallId { get; set; }
	public int MovieId { get; set; }
}

public class ShowtimeCreateDTO
{
	public DateTime StartTime { get; set; }
	public int CinemaHallId { get; set; }
	public int MovieId { get; set; }

	public Showtime ToShowtime()
	{
		return new Showtime
		{
			StartTime = StartTime,
			CinemaHallId = CinemaHallId,
			MovieId = MovieId,
		};
	}
}
