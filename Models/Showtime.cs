using System.ComponentModel.DataAnnotations.Schema;

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
	public ICollection<Seat> Seats { get; set; } = [];

	[NotMapped]
	public bool HasReservations {get => Reservations.Count > 0;}

	public void InitializeSeats()
	{
		if (CinemaHall == null)
		{
			throw new InvalidOperationException("CinemaHall is null");
		}

		Seats = Seats.Where(s => !s.IsAvailable).ToList();
		for(int i = 0; i < CinemaHall!.Capacity; i++)
		{
			Seats.Add(new Seat
			{
				Number = i + 1,
				IsAvailable = true,
				CinemaHallId = CinemaHallId
			});
		}
	}

}