using MovieRental.Common;

namespace MovieRental.Customer;

public interface ICustomerFeatures
{
    Task<Response<Customer>> GetCustomerByIdAsync(int customerId);
    Task<Response<Customer>> SaveCustomerAsync(Customer customer);
}