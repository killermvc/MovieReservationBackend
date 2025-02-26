using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using MovieReservation.Models;

namespace MovieReservation.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ReportsController(AppDbContext _db) : ControllerBase
{

	[HttpGet]
	[Authorize(Roles = "Admin")]
	public  IActionResult RevenueReport([FromQuery] RevenueReportDTO dates)
	{

		if(dates.StartDate > dates.EndDate)
		{
			return BadRequest("Start date must be before end date");
		}

		var reservations = _db.Reservations
			.Include(r => r.Showtime)
				.ThenInclude(s => s!.CinemaHall)
			.Include(r => r.Showtime)
				.ThenInclude(s => s!.Movie)
			.Where(r => r.Showtime!.StartTime > dates.StartDate
				&& r.Showtime.StartTime <= dates.EndDate)
			.GroupBy(r => r.Showtime!.CinemaHall);

		var result = reservations
			.Select(g => new {
				g.Key!.Id,
				g.Key!.Name,
				Total = g.Sum(r => r.Seats.Count() * r.Showtime!.Movie!.TicketPrice) })
			.ToList();
		result.Add(new {Id = -1, Name = "Total", Total = result.Sum(r => r.Total)});

		return Ok(result);
	}

}

public class RevenueReportDTO
{
	public DateTime StartDate {get; set;}
	public DateTime EndDate {get; set;}
}