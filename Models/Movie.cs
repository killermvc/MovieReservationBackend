using System.ComponentModel.DataAnnotations;

namespace MovieReservation.Models;

public class Movie
{
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public Genre? Genre { get; set; }
	public string PosterUrl = string.Empty;
	public int DurationInMinutes {get; set;}
	public decimal TicketPrice { get; set; }
	public DateTime ReleaseDate { get; set; }
	public ICollection<Showtime> Showtimes { get; set; } = [];
}