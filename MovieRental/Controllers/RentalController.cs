using Microsoft.AspNetCore.Mvc;
using MovieRental.Common;
using MovieRental.Movie;
using MovieRental.Rental;

namespace MovieRental.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RentalController : ControllerBase
    {
        private readonly IRentalFeatures _features;
        private readonly ILogger<RentalController> _logger;

        public RentalController(IRentalFeatures features, ILogger<RentalController> logger)
        {
            _features = features;
            _logger = logger;
        }

        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{rentalId:int}", Name = "GetRentalByIdAsync")]
        public async Task<IActionResult> GetRentalByIdAsync(int? rentalId)
        {
            var response = new Response<Rental.Rental>();

            try 
            {
                if (rentalId == null || rentalId <= 0)
                {
                    _logger.LogError("Invalid rental Id: {rentalId}", rentalId);
                    return BadRequest("Rental Id must be a positive integer.");
                }

                response = await _features.GetRentalByIdAsync(rentalId.Value);

                if (!response.isSuccess)
                {
                    _logger.LogError("Rental with Id {rentalId} not found.", rentalId);
                    return NotFound(response.GetErrors());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating rental ID {rentalId}.", rentalId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }

            return Ok(response.GetData());
        }

        //[Produces("application/json")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[HttpGet("{customerName:string}")]
        //public async Task<IActionResult> GetRentalsByCustomerNameAsync(string customerName)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(customerName))
        //        {
        //            return BadRequest("Customer name cannot be null or empty.");
        //        }

        //        var rentals = await _features.GetRentalsByCustomerNameAsync(customerName);

        //        return Ok(rentals);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while retrieving rentals for customer '{customerName}'.", customerName);
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving rentals for customer '{customerName}'.");
        //    }
        //}

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateRentalAsync([FromBody] Rental.Rental rental)
        {
            try
            {
                if (rental == null)
                {
                    return BadRequest("Rental cannot be null");
                }

                if (rental.DaysRented <= 0)
                {
                    return BadRequest("DaysRented must be a positive integer.");
                }

                if (rental.MovieId <= 0)
                {
                    return BadRequest("MovieId must be a positive integer.");
                }

                if (rental.CustomerId <= 0)
                {
                    return BadRequest("CustomerId must be a positive integer.");
                }

                // TODO: Mapping

                var response = await _features.SaveRentalAsync(rental);

                if (!response.isSuccess)
                {
                    return BadRequest(response.GetErrors());
                }

                return CreatedAtRoute("GetRentalByIdAsync", new { rentalId = response.GetData()!.Id }, response.GetData());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the rental.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving the rental.");
            }
        }
	}
}
