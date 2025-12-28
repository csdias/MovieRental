using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieRental.Common;
using MovieRental.Data;
using MovieRental.PaymentProviders;
using System.Text.Json;

namespace MovieRental.Rental
{
	public class RentalFeatures : IRentalFeatures
	{
		private readonly MovieRentalDbContext _movieRentalDbCtx;
        private readonly IPaymentProviderFactory _paymentProviderFactory;
        private readonly ILogger<RentalFeatures> _log;

        public RentalFeatures(MovieRentalDbContext movieRentalDb, 
            IPaymentProviderFactory paymentProviderFactory,
            ILogger<RentalFeatures> log)
		{
			_movieRentalDbCtx = movieRentalDb;
            _paymentProviderFactory = paymentProviderFactory;
            _log = log;
        }

        public async Task<Response<Rental>> GetRentalByIdAsync(int rentalId)
        {
            var response = new Response<Rental>();
            try
            {
                if (rentalId <= 0)
                {
                    _log.LogWarning("Invalid rentalId {rentalId} provided.", rentalId);
                    response.AddError("The rentalId must be a positive integer.");
                    return response;
                }

                var rental = await _movieRentalDbCtx.Rentals
                    .FirstOrDefaultAsync(r => r.Id == rentalId);

                if (rental != null)
                {
                    response.SetData(rental);
                }
                else
                {
                    _log.LogWarning("Rental with Id {rentalId} not found.", rentalId);
                    response.AddError($"Rental with Id {rentalId} not found.");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while retrieving rental with Id {rentalId}.", rentalId);
                response.AddError($"An error occurred while retrieving rental with Id {rentalId}.");
            }

            return response;
        }

        //TODO: finish this method and create an endpoint for it
        public async Task<Response<IEnumerable<Rental>>> GetRentalsByCustomerNameAsync(string customerName)
		{
            var response = new Response<IEnumerable<Rental>>();

            try
            {
                if (string.IsNullOrWhiteSpace(customerName))
                {
                    _log.LogWarning("Customer name is null or empty.");
                    response.AddError("Customer name must be provided.");
                    return response;
                }

                var rentals = await _movieRentalDbCtx.Rentals
					.Where(r => r.Customer != null && r.Customer.Name == customerName)
					.ToListAsync();

				if (!rentals.Any())
				{
					response.SetData(rentals);
				}
				else
				{
					_log.LogWarning("No rentals found for customer '{customerName}'.", customerName);
                    response.AddError($"No rentals found for the customer '{customerName}'.");
				}
			}
			catch (Exception ex)
			{
				_log.LogError(ex, "An error occurred while retrieving rentals for customer '{customerName}'.", customerName);
				response.AddError("An unexpected error occurred while processing your request.");
            }

            return response;
        }

        public async Task<Response<IEnumerable<Rental>>> GetPaginatedRentalsAsync(int pageSize, int pageNumber)
        {
            var response = new Response<IEnumerable<Rental>>();
            try
            {
                if (pageSize <= 0 || pageNumber <= 0)
                {
                    _log.LogWarning("Invalid pagination parameters: pageSize={pageSize}, pageNumber={pageNumber}", pageSize, pageNumber);
                    response.AddError("Page size and page number must be positive integers.");
                    return response;
                }
                var rentals = await _movieRentalDbCtx.Rentals
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                response.SetData(rentals);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while retrieving paginated rentals: pageSize={pageSize}, pageNumber={pageNumber}", pageSize, pageNumber);
                response.AddError("An unexpected error occurred while processing your request.");
            }
            return response;
        }

        private async Task<IList<string>> ValidateRentalOnSaving(Rental rental)
        {
            var errors = new List<string>();

            if (rental == null)
            {
                var msg = "Rental data must be provided.";
                _log.LogError(msg);
                errors.Add(msg);
                return errors;
            }

            if (rental.DaysRented <= 0)
            {
                var msg = $"Invalid DaysRented {rental.DaysRented} provided for rental.";
                _log.LogError(msg, rental.DaysRented);
                errors.Add(msg);
            }

            if (string.IsNullOrWhiteSpace(rental.PaymentMethod))
            {
                var msg = "PaymentMethod must be provided.";
                _log.LogError(msg);
                errors.Add(msg);
            }

            if (rental.MovieId <= 0)
            {
                var msg = "MovieId must be a positive integer.";
                _log.LogError(msg, rental.MovieId);
                errors.Add(msg);
            }

            if (!await _movieRentalDbCtx.Movies.AsNoTracking().AnyAsync(m => m.Id == rental.MovieId))
            {
                var msg = $"Movie with Id {rental.MovieId} does not exist.";
                _log.LogWarning(msg, rental.MovieId);
                errors.Add(msg);
            }

            if (!await _movieRentalDbCtx.Customers.AsNoTracking().AnyAsync(c => c.Id == rental.CustomerId))
            {
                var msg = $"Customer with Id {rental.CustomerId} does not exist.";
                _log.LogError(msg, rental.CustomerId);
                errors.Add(msg);
            }

            return errors;
        }

        // TODO: Make me async :(
        // TODO: Explain to us what is the difference?
        // Answer: In this specific scenario, when we use async method, we release the ASP.NET
        // Thread Pool to handle other requests while waiting for the database / network operation to complete.
        // This improves the scalability and responsiveness of the application, especially under high load.
        public async Task<Response<Rental>> SaveRentalAsync(Rental rental)
        {
            var response = new Response<Rental>();

            try
            {
                var validationErrors = await ValidateRentalOnSaving(rental);

                if (validationErrors.Any())
                {
                    foreach (var error in validationErrors)
                    {
                        response.AddError(error);
                    }
                    return response;
                }
                
                var paymentProvider = _paymentProviderFactory.GetPaymentProvider(rental.PaymentMethod);

                double pricePerDay = 3.75; // This could be dynamic based on movie or other factors

                var isPaymentSuccess = await paymentProvider.Pay(rental.DaysRented * pricePerDay);

                if (!isPaymentSuccess)
                {
                    var msg = $"Payment processing failed for PaymentMethod {rental.PaymentMethod}. The rental was not completed.";
                    _log.LogError(msg, rental.PaymentMethod);
                    response.AddError(msg);
                    return response;
                }

                // TODO: Separate the RequestSaveRental from the Entity
                rental.Customer = null; // To avoid EF trying to insert a new Customer
                rental.Movie = null;    // To avoid EF trying to insert a new Movie

                _movieRentalDbCtx.Rentals.Add(rental);

                var result = await _movieRentalDbCtx.SaveChangesAsync();

                var jsonObject = JsonSerializer.Serialize(rental);

                if (result == 0)
                {
                    _log.LogInformation("Nothing was saved for {@jsonObject}", jsonObject); // TODO: Test this {@jsonObject}
                }

                response.SetData(rental);
            }
            catch (Exception ex)
            {
                var jsonObject = JsonSerializer.Serialize(rental);
                var msg = $"An error occurred while saving the rental { jsonObject }";
                _log.LogError(ex, msg, jsonObject);
                response.AddError(msg);
            }
            
            return response;
        }
    }
}
