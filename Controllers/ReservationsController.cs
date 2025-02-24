using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using MovieReservation.Models;

namespace MovieReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(AppDbContext _db) : ControllerBase
{
	[HttpGet("{id}")]
	[Authorize]
	public async Task<IActionResult> GetById(int id)
	{
		var reservation = await _db.Reservations.FindAsync(id);
		if (reservation == null)
		{
			return NotFound();
		}

		if (User.Identity is not null
			&& reservation.UserId != User.Identity.Name
			&& !User.IsInRole("Admin"))
		{
			return Forbid("Not authorized");
		}

		return Ok(reservation);
	}

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<IEnumerable<Reservation>>> GetAll(int pageNumber = 1, int pageSize = 10, bool activeOnly = true)
	{
		if(User.Identity is null)
		{
			return Unauthorized();
		}
		var userId = User.Identity.Name;

		IQueryable<Reservation> reservations;

		if(User.IsInRole("Admin"))
		{
			reservations = _db.Reservations
				.Include(r => r.Showtime);
			if(activeOnly)
			{
				reservations = reservations.Where(r => r.IsActive);
			}
		}
		else
		{
			reservations = _db.Reservations
				.Include(r => r.Showtime)
				.Where(r => r.UserId == userId && r.IsActive);
		}

		var result = await reservations
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

		return Ok(result);
	}

	[HttpPost]
	[Authorize]
	public async Task<IActionResult> Create([FromBody] ReservationCreateDTO createDTO)
	{
		if (User.Identity is null || User.Identity.Name == null)
		{
			return Unauthorized();
		}

		var showtime = await _db.Showtimes.FindAsync(createDTO.ShowtimeId);
		if (showtime == null)
		{
			return BadRequest("Invalid ShowtimeId");
		}

		List<Seat> seats = [];

		foreach(var s in createDTO.Seats)
		{
			var requestedSeat = await _db.Seats.FirstOrDefaultAsync(seat => seat.Id == s);
			if(requestedSeat == null)
			{
				return BadRequest($"Seat with Id {s} does not exist for showtime {createDTO.ShowtimeId}");
			}

			if(!requestedSeat.IsAvailable)
			{
				return BadRequest($"Seat {s} is not available");
			}
			requestedSeat.IsAvailable = false;
			seats.Add(requestedSeat);
		}

		var reservation = new Reservation
		{
			UserId = User.Identity.Name,
			ShowtimeId = createDTO.ShowtimeId,
			Seats = seats,
			IsActive = true
		};

		await _db.Reservations.AddAsync(reservation);
		await _db.SaveChangesAsync();

		return CreatedAtAction(nameof(GetAll), new { id = reservation.Id }, reservation);
	}

	[HttpDelete("{id}")]
	[Authorize]
	public async Task<IActionResult> Cancel(int id)
	{
		var reservation = await _db.Reservations
			.Include(r => r.Showtime)
			.Include(r => r.Seats)
			.FirstOrDefaultAsync(r => r.Id == id);
		if (reservation == null)
		{
			return NotFound();
		}

		if (User.Identity is not null
			&& reservation.UserId != User.Identity.Name
			&& !User.IsInRole("Admin"))
		{
			return Forbid("Not authorized");
		}

		if(reservation.Showtime!.StartTime < DateTime.Now)
		{
			return BadRequest("Can't cancel reservation for past showtime");
		}

		reservation.IsActive = false;
		foreach(var seat in reservation.Seats)
		{
			seat.IsAvailable = true;
		}

		await _db.SaveChangesAsync();
		return NoContent();
	}

}

public class ReservationCreateDTO
{
    public int ShowtimeId { get; set; }
    public IEnumerable<int> Seats { get; set; } = [];

}