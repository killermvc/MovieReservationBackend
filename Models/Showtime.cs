namespace MovieReservation.Models;

public class Showtime
{
	public int Id { get; set; }
	public DateTime StartTime { get; set; }
	public int CinemaHallId { get; set; }
	public CinemaHall? CinemaHall { get; set; }
	public int MovieId { get; set; }
	public Movie? Movie { get; set; }

	public ICollection<Reservation> Reservations { get; set; } = [];
}