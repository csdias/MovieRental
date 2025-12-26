using Microsoft.AspNetCore.Mvc;
using MovieRental.Common;
using MovieRental.Movie;

namespace MovieRental.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieFeatures _features;
        ILogger<MovieController> _log;

        public MovieController(IMovieFeatures features, ILogger<MovieController> log)
        {
            _features = features;
            _log = log;
        }

        //[Produces("application/json")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[HttpGet]
        //public async Task<IActionResult> GetPaginatedMoviesAsync([FromQuery] RequestParameters input)
        //{
        //    var movies = new List<Movie.Movie>();

        //    try 
        //    {
        //        if (input == null || input.PageNumber == null || input.PageSize == null)
        //        {
        //            return BadRequest("PageNumber and PageSize are required.");
        //        }

        //        var response = await _features.GetPaginatedMoviesAsync(input.PageNumber.Value, input.PageSize.Value);

        //        if (!response.isSuccess)
        //        {
        //            var errors = string.Join(", ", response?.GetErrors() ?? new List<Error>());
        //            _log.LogError("Failed to get movies: {errors}", errors);
        //            return StatusCode(500);
        //        }

        //        movies = response.GetData()?.ToList();

        //    }
        //    catch (Exception ex)
        //    {
        //        _log.LogError(ex, "An error occurred while getting all movies.");
        //        return StatusCode(500);
        //    }

        //    return Ok(movies);
        //}

        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{movieId:int}", Name = "GetMovieByIdAsync")]
        public async Task<IActionResult> GetMovieByIdAsync(int? movieId)
        {
            var response = new Response<Movie.Movie>();

            try
            {
                if (movieId == null || movieId <= 0)
                {
                    return BadRequest("Movie Id must be a positive integer.");
                }

                response = await _features.GetMovieByIdAsync(movieId.Value);

                if (!response.isSuccess)
                {
                    return NotFound($"Movie with Id {movieId} not found.");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while getting the movie by Id.");
                return StatusCode(500);
            }

            return Ok(response.GetData());
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateMovieAsync([FromBody] Movie.Movie movie)
        {
            try 
            {
                if (movie == null)
                {
                    return BadRequest("Movie cannot be null");
                }

                if (string.IsNullOrWhiteSpace(movie.Title))
                {
                    return BadRequest("Movie title is required.");
                }

                if (movie.Id < 0) // TODO: Find a better id mechanism
                {
                    return BadRequest("Movie Id must be greater then zero");
                }

                // TODO: Add mapping

                var response = await _features.SaveMovieAsync(movie);

                if (!response.isSuccess)
                {
                    return BadRequest(response.GetErrors());
                }

                var id = response?.GetData()?.Id ?? 0;

                return CreatedAtRoute("GetMovieByIdAsync", new { movieId = id }, response?.GetData());
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while saving the movie.");
                return StatusCode(500);
            }
        }
    }
}
