namespace MovieReservation.Models;

public class CinemaHall
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public int Capacity { get; set; }
	public int CinemaId { get; set; }
	public Cinema? Cinema { get; set; }
}