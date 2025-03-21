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
	public ActionResult<IEnumerable<ShowtimeDTO>> GetAllForMovie(int movieId)
	{
		var showtimes = _db.Showtimes
			.Where(s => s.MovieId == movieId && s.StartTime > DateTime.Now)
			.ToList();
		return Ok(showtimes.Select(s => new ShowtimeDTO(s)));
	}

	[HttpGet("day/{date}")]
	public ActionResult<IEnumerable<ShowtimeDTO>> GetAllForDay(DateTime date)
	{
		if(date < DateTime.Now)
		{
			return BadRequest("Date should be in the future");
		}
		var showtimes = _db.Showtimes
			.Where(s => s.StartTime.Date == date.Date)
			.ToList();
		return Ok(showtimes.Select(s => new ShowtimeDTO(s)));
	}

	[HttpGet("movie/{movieId}/day/{date}")]
	public ActionResult<IEnumerable<ShowtimeDTO>> GetAllForMovieForDay(int movieId, DateTime date)
	{
		var showtimes = _db.Showtimes
			.Where(s =>	s.MovieId == movieId && s.StartTime.Date == date.Date)
			.ToList();
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

		var cinemaHall = await _db.CinemaHalls.FindAsync(showtime.CinemaHallId);
		showtime.CinemaHall = cinemaHall;
		showtime.InitializeSeats();

		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = showtime.Id }, new ShowtimeDTO(showtime));
	}

	[HttpPut("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Update(int id, [FromBody] ShowtimeUpdateDTO updateDTO)
	{
		var showtime = await _db.Showtimes.FindAsync(id);
		if (showtime == null)
		{
			return NotFound();
		}

		var oldStartTime = showtime.StartTime;
		if (Math.Abs((updateDTO.StartTime - oldStartTime).TotalMinutes) > 15)
		{
			return BadRequest("StartTime should not be changed by more than 15 minutes");
		}
		if(updateDTO.StartTime < DateTime.Now)
		{
			return BadRequest("StartTime should be in the future");
		}

		var oldCinemaHall = showtime.CinemaHallId;
		if (showtime.HasReservations && oldCinemaHall != updateDTO.CinemaHallId)
		{
			return BadRequest("Cannot update showtime with reservations");
		}

		var oldMovieId = showtime.MovieId;
		if (showtime.HasReservations && oldMovieId != updateDTO.MovieId)
		{
			return BadRequest("Cannot update showtime with reservations");
		}

		showtime.StartTime = updateDTO.StartTime;
		showtime.CinemaHallId = updateDTO.CinemaHallId;
		showtime.MovieId = updateDTO.MovieId;


		await _db.SaveChangesAsync();
		return Ok(new ShowtimeDTO(showtime));
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
