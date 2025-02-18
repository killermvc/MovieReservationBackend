using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MovieReservation.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
{

	public DbSet<Movie> Movies { get; set; } = null!;
	public DbSet<Cinema> Cinemas { get; set; } = null!;
	public DbSet<CinemaHall> CinemaHalls { get; set; } = null!;
	public DbSet<Reservation> Reservations { get; set; } = null!;
	public DbSet<Showtime> Showtimes { get; set; } = null!;
	public DbSet<Seat> Seats { get; set; } = null!;
	public DbSet<Genre> Genres { get; set; } = null!;
	public DbSet<Showtime> Showtime { get; set; } = null!;
}