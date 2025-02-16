using System.ComponentModel.DataAnnotations.Schema;

namespace MovieReservation.Models;

public class Reservation
{
	public int Id { get; set; }
	public int ShowtimeId { get; set; }
	public Showtime? Showtime { get; set; }
	public int SeatId { get; set; }
	public Seat? Seat { get; set; }
	public string UserId { get; set;} = string.Empty;
	public User? User { get; set; }
}