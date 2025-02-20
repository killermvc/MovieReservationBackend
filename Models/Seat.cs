namespace MovieReservation.Models;

public class Seat
{
	public int Id { get; set; }
	public int Number {get; set;}
	public bool IsAvailable {get; set;}
	public int CinemaHallId {get; set;}
	public CinemaHall? CinemaHall {get; set;}
	public int ShowtimeId {get; set;}
	public Showtime? Showtime {get; set;}
}