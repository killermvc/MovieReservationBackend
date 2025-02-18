using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using MovieReservation.Models;

namespace MovieReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController(AppDbContext _db) : ControllerBase
{
	[HttpGet]
	[Authorize]
	public ActionResult<IEnumerable<MovieDTO>> GetAll(int pageNumber = 1, int pageSize = 10)
	{
		var movies = _db.Movies
			.OrderBy(m => m.Title)
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToList();
		var movieDTOs = movies.Select(m => new MovieDTO(m));
		return Ok(movieDTOs);
	}

	[HttpGet("genre/{genreId}")]
	[Authorize]
	public ActionResult<IEnumerable<MovieDTO>> GetAllByGenre(int genreId)
	{
		var movies = _db.Movies.Where(m => m.GenreId == genreId).ToList();
		if (movies.Count == 0)
		{
			return NotFound();
		}
		var movieDTOs = movies.Select(m => new MovieDTO(m));
		return Ok(movieDTOs);
	}


	[HttpGet("{id}")]
	[Authorize]
	public async Task<ActionResult<MovieDTO>> GetById(int id)
	{
		var movie = await _db.Movies.FindAsync(id);
		if (movie == null)
		{
			return NotFound();
		}
		return Ok(new MovieDTO(movie));
	}

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult<MovieDTO>> Create(MovieCreateDTO createDTO)
	{
		var movie = createDTO.ToMovie();
		await _db.Movies.AddAsync(movie);
		await _db.SaveChangesAsync();

		return CreatedAtAction(nameof(GetById), new { id = movie.Id }, new MovieDTO(movie));
	}

	[HttpPut("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult> Update(int id, MovieUpdateDTO updateDTO)
	{
		var movie = await _db.Movies.FindAsync(id);
		if (movie == null)
		{
			return NotFound();
		}

		movie.Title = updateDTO.Title;
		movie.Description = updateDTO.Description;
		movie.ReleaseDate = updateDTO.ReleaseDate;
		movie.GenreId = updateDTO.GenreId;
		movie.PosterUrl = updateDTO.PosterUrl;
		movie.DurationInMinutes = updateDTO.DurationInMinutes;
		movie.TicketPrice = updateDTO.TicketPrice;

		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpDelete("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult> Delete(int id)
	{
		var movie = await _db.Movies.FindAsync(id);
		if (movie == null)
		{
			return NotFound();
		}

		_db.Movies.Remove(movie);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}

public class MovieDTO(Movie movie)
{
	public int Id { get; init; } = movie.Id;
	public string Title { get; init; } = movie.Title;
	public string Description { get; init; } = movie.Description;
	public DateTime ReleaseDate { get; init; } = movie.ReleaseDate;
	public int GenreId { get; init; } = movie.GenreId;
	public string PosterUrl { get; init; } = movie.PosterUrl;
	public int DurationInMinutes { get; init; } = movie.DurationInMinutes;
	public decimal TicketPrice { get; init; } = movie.TicketPrice;
}

public class MovieCreateDTO
{
	public string Title { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
	public DateTime ReleaseDate { get; init; }
	public int GenreId { get; init; }
	public string PosterUrl { get; init; } = string.Empty;
	public int DurationInMinutes { get; init; }
	public decimal TicketPrice { get; init; }

	public Movie ToMovie()
	{
		return new Movie
		{
			Title = Title,
			Description = Description,
			ReleaseDate = ReleaseDate,
			GenreId = GenreId,
			PosterUrl = PosterUrl,
			DurationInMinutes = DurationInMinutes,
			TicketPrice = TicketPrice
		};
	}
}

public class MovieUpdateDTO
{
	public string Title { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
	public DateTime ReleaseDate { get; init; }
	public int GenreId { get; init; }
	public string PosterUrl { get; init; } = string.Empty;
	public int DurationInMinutes { get; init; }
	public decimal TicketPrice { get; init; }
}

