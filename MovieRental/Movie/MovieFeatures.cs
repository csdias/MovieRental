using Microsoft.EntityFrameworkCore;
using MovieRental.Common;
using MovieRental.Data;
using MovieRental.Rental;
using System.Text.Json;

namespace MovieRental.Movie
{
	public class MovieFeatures : IMovieFeatures
	{
		private readonly MovieRentalDbContext _movieRentalDbCtx;
		private readonly ILogger<MovieFeatures> _log;

        public MovieFeatures(MovieRentalDbContext movieRentalDb, ILogger<MovieFeatures> log)
		{
			_movieRentalDbCtx = movieRentalDb;
			_log = log;
        }

        public async Task<Response<Movie>> GetMovieByIdAsync(int movieId)
        {
            var response = new Response<Movie>();
            try
            {
                var rental = await _movieRentalDbCtx.Movies
                    .FirstOrDefaultAsync(r => r.Id == movieId);

                if (rental != null)
                {
                    response.SetData(rental);
                }
                else
                {
                    var msg = $"Movie with Id {movieId} not found.";
                    _log.LogWarning(msg, movieId);
                    response.AddError(msg);
                }
            }
            catch (Exception ex)
            {
                var msg = $"An error occurred while retrieving movie with Id {movieId}.";
                _log.LogError(ex, msg, movieId);
                response.AddError(msg);
            }

            return response;
        }

        public async Task<Response<IEnumerable<Movie>>> GetMoviesByTitleAsync(string movieTitle)
        {
            var response = new Response<IEnumerable<Movie>>();
            try
            {
                if (string.IsNullOrWhiteSpace(movieTitle))
                {
                    var msg = "Movie title cannot be null or empty.";
                    _log.LogWarning(msg);
                    response.AddError(msg);
                    return response;
                }

                var movies = await _movieRentalDbCtx.Movies
                    .Where(m => m.Title.Contains(movieTitle))
                    .AsNoTracking()
                    .ToListAsync();
                response.SetData(movies);
            }
            catch (Exception ex)
            {
                var msg = $"An error occurred while retrieving movies with name containing '{movieTitle}'.";
                _log.LogError(ex, msg, movieTitle);
                response.AddError(msg);
            }
            return response;
        }

        // TODO: tell us what is wrong in this method? Forget about the async, what other concerns do you have?
        // Answer: This method retrieves all movies from the database and loads them into memory, which can be inefficient
        // for large datasets. 
        // Ideally, we should implement pagination to limit the number of records returned in a single call.
        // In the query to return to the caller, we should consider using IQueryable<Movie> instead of List<Movie>
        // to allow for further filtering and querying by the caller. IQueryable supports deferred execution,
        // which can improve performance.
        // Additionally, there is no error handling; if the database connection fails, it could lead to unhandled exceptions.
        // Also, consider using asynchronous operations to improve scalability.
        // Finally, returning a List<Movie> directly may not be ideal for API responses; consider using DTOs or view models.
        public async Task<Response<IEnumerable<Movie>>> GetPaginatedMoviesAsync(int pageSize = 10, int pageNumber = 1)
		{
            var response = new Response<IEnumerable<Movie>>();
            try
            {
                // TODO: Implement a centralized validation

                if (pageSize <= 0)
                {
                    _log.LogWarning("Invalid pageSize {pageSize} provided.", pageSize);
                    response.AddError("Page size must be a positive integer.");
                }
                if (pageNumber <= 0)
                {
                    _log.LogWarning("Invalid page {page} provided.", pageNumber);
                    response.AddError("Page number must be a positive integer.");
                }

                var movies = await _movieRentalDbCtx.Movies
                    .Skip((pageNumber - 1) * pageSize) // TODO: Test me
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                response.SetData(movies);
            }
            catch (Exception ex)
            {
                var msg = "An error occurred while retrieving movies.";
                _log.LogError(ex, msg);
                response.AddError(msg);
            }

            return response;
        }

        public async Task<Response<Movie>> SaveMovieAsync(Movie movie)
        {
            var response = new Response<Movie>();

            try
            {
                _movieRentalDbCtx.Movies.Add(movie);
                var result = await _movieRentalDbCtx.SaveChangesAsync();

                var jsonObject = JsonSerializer.Serialize(movie);
                if (result == 0)
                {
                    _log.LogInformation("Nothing was saved for {@jsonObject}", jsonObject);
                }
                response.SetData(movie);
            }
            catch (Exception ex)
            {
                var msg = "An error occurred while saving the movie.";
                _log.LogError(ex, msg);
                response.AddError(msg);
            }

            return response;
        }

    }
}
