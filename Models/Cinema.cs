namespace MovieReservation.Models;

public class Cinema
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
	public ICollection<CinemaHall> CinemaHalls { get; set; } = [];
}