using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


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

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (userId is not null
			&& reservation.UserId != userId
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
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if(userId == null)
		{
			return BadRequest("User not found");
		}

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
		var showtime = await _db.Showtimes
			.Include(s => s.Seats)
			.FirstOrDefaultAsync(s => s.Id == createDTO.ShowtimeId);

		if (showtime == null)
		{
			return BadRequest("Invalid ShowtimeId");
		}

		List<Seat> seats = [];

		foreach(var s in createDTO.Seats)
		{
			var requestedSeat = await _db.Seats.FirstOrDefaultAsync(seat =>
				seat.Number == s
				&& seat.ShowtimeId == createDTO.ShowtimeId);
			if(requestedSeat == null)
			{
				return BadRequest($"[{_db.Seats.Count()}]Seat with Id {s} does not exist for showtime {createDTO.ShowtimeId}");
			}

			if(!requestedSeat.IsAvailable)
			{
				return BadRequest($"Seat {s} is not available");
			}
			requestedSeat.IsAvailable = false;
			seats.Add(requestedSeat);
		}

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if(userId == null)
		{
			return Unauthorized();
		}

		var reservation = new Reservation
		{
			UserId = userId,
			ShowtimeId = createDTO.ShowtimeId,
			Seats = seats,
			IsActive = true
		};

		await _db.Reservations.AddAsync(reservation);
		await _db.SaveChangesAsync();

		return CreatedAtAction(
			nameof(GetAll),
			new { id = reservation.Id },
			new ReservationDTO(reservation));
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

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is not null
			&& reservation.UserId != userId
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

public class ReservationDTO
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int ShowtimeId { get; set; }
    public string Seats { get; set; }
    public bool IsActive { get; set; }

	public ReservationDTO(Reservation reservation)
	{
		Id = reservation.Id;
		UserId = reservation.UserId;
		ShowtimeId = reservation.ShowtimeId;
		Seats = string.Join(",", reservation.Seats.Select(s => s.Number));
		IsActive = reservation.IsActive;
	}
}