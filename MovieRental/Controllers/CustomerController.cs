using Microsoft.AspNetCore.Mvc;
using MovieRental.Common;
using MovieRental.Customer;

namespace MovieRental.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerFeatures _features;
        ILogger<CustomerController> _log;

        public CustomerController(ICustomerFeatures features, ILogger<CustomerController> log)
        {
            _features = features;
            _log = log;
        }

        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{customerId:int}", Name = "GetCustomerByIdAsync")]
        public async Task<IActionResult> GetCustomerByIdAsync(int? customerId)
        {
            var response = new Response<Customer.Customer>();

            try
            {
                if (customerId == null || customerId <= 0)
                {
                    return BadRequest("Customer Id must be a positive integer.");
                }

                response = await _features.GetCustomerByIdAsync(customerId.Value);

                if (!response.isSuccess)
                {
                    return NotFound($"Customer with Id {customerId} not found.");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while getting the customer by Id.");
                return StatusCode(500);
            }

            return Ok(response.GetData());
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateCustomerAsync([FromBody] Customer.Customer customer)
        {
            try 
            {

                if (customer == null)
                {
                    return BadRequest("Movie cannot be null");
                }

                if (string.IsNullOrWhiteSpace(customer.Name))
                {
                    return BadRequest("Customer name is required.");
                }

                if (customer.Id < 0) // TODO: Find a better id mechanism
                {
                    return BadRequest("Customer Id must be greater then zero");
                }

                // TODO: Add mapping

                var response = await _features.SaveCustomerAsync(customer);

                if (!response.isSuccess)
                {
                    return BadRequest(response.GetErrors());
                }

                var id = response?.GetData()?.Id ?? 0;

                return CreatedAtRoute("GetCustomerByIdAsync", new { customerId = id }, response?.GetData()); // TODO: Verify
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while saving the customer.");
                return StatusCode(500);
            }
        }
    }
}
