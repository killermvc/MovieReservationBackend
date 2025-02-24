using System.ComponentModel.DataAnnotations.Schema;

namespace MovieReservation.Models;

public class Reservation
{
	public int Id { get; set; }
	public int ShowtimeId { get; set; }
	public Showtime? Showtime { get; set; }
	public IEnumerable<Seat> Seats { get; set; } = [];
	public string UserId { get; set;} = string.Empty;
	public User? User { get; set; }
	public bool IsActive { get; set; } = true;
}