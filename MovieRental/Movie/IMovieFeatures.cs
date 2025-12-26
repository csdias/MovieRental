using MovieRental.Common;

namespace MovieRental.Movie;

public interface IMovieFeatures
{
    Task<Response<Movie>> GetMovieByIdAsync(int movieId);
    Task<Response<IEnumerable<Movie>>> GetMoviesByTitleAsync(string movieTitle);
    Task<Response<IEnumerable<Movie>>> GetPaginatedMoviesAsync(int pageSize = 10, int pageNumber = 1);
    Task<Response<Movie>> SaveMovieAsync(Movie movie);
}