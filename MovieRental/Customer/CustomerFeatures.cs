using Microsoft.EntityFrameworkCore;
using MovieRental.Common;
using MovieRental.Data;
using MovieRental.Customer;
using System.Text.Json;

namespace MovieRental.Customer
{
	public class CustomerFeatures : ICustomerFeatures
	{
		private readonly MovieRentalDbContext _movieRentalDbCtx;
		private readonly ILogger<CustomerFeatures> _log;

        public CustomerFeatures(MovieRentalDbContext movieRentalDb, ILogger<CustomerFeatures> log)
		{
			_movieRentalDbCtx = movieRentalDb;
			_log = log;
        }

        public async Task<Response<Customer>> GetCustomerByIdAsync(int customerId)
        {
            var response = new Response<Customer>();
            try
            {
                var rental = await _movieRentalDbCtx.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == customerId);

                if (rental != null)
                {
                    response.SetData(rental);
                }
                else
                {
                    var msg = $"Customer with Id {customerId} not found.";
                    _log.LogWarning(msg, customerId);
                    response.AddError(msg);
                }
            }
            catch (Exception ex)
            {
                var msg = $"An error occurred while retrieving customer with Id {customerId}.";
                _log.LogError(ex, msg, customerId);
                response.AddError(msg);
            }

            return response;
        }
        public async Task<Response<Customer>> SaveCustomerAsync(Customer customer)
        {
            var response = new Response<Customer>();

            try
            {
                _movieRentalDbCtx.Customers.Add(customer);
                var result = await _movieRentalDbCtx.SaveChangesAsync();

                var jsonObject = JsonSerializer.Serialize(customer);
                if (result == 0)
                {
                    _log.LogInformation("Nothing was saved for {@jsonObject}", jsonObject);
                }
                response.SetData(customer);
            }
            catch (Exception ex)
            {
                var msg = "An error occurred while saving the customer.";
                _log.LogError(ex, msg);
                response.AddError(msg);
            }

            return response;
        }

    }
}
