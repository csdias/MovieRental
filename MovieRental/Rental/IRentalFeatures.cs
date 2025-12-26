using MovieRental.Common;

namespace MovieRental.Rental;

public interface IRentalFeatures
{
	Task<Response<Rental>> GetRentalByIdAsync(int rentalId);
	Task<Response<IEnumerable<Rental>>> GetRentalsByCustomerNameAsync(string customerName);
    Task<Response<IEnumerable<Rental>>> GetPaginatedRentalsAsync(int pageSize, int pageNumber);
    Task<Response<Rental>> SaveRentalAsync(Rental rental);
}