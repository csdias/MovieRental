using Microsoft.EntityFrameworkCore;
using MovieRental.Customer;
using MovieRental.Data;

namespace MovieRentalUnitTests
{
    public class DatabaseFixture : IDisposable
    {
        public required MovieRentalDbContext DataContext;

        public void Initialize()
        {
            var options = new DbContextOptionsBuilder<MovieRentalDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            DataContext = new MovieRentalDbContext(options);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            DataContext.Customers.AddRange([
                new Customer { Id = 1, Name = "Jhon Doe" },
                new Customer { Id = 2, Name = "Jane Smith" }
            ]);

            DataContext.Movies.AddRange([
                new MovieRental.Movie.Movie { Id = 1, Title = "Movie One" },
                new MovieRental.Movie.Movie { Id = 2, Title = "Movie Two" },
                new MovieRental.Movie.Movie { Id = 3, Title = "Movie Three"} 
            ]);

            DataContext.Rentals.AddRange([
                new MovieRental.Rental.Rental { Id = 1, MovieId = 1, CustomerId = 1, 
                    DaysRented = 2, PaymentMethod = "MbWay" },
                new MovieRental.Rental.Rental { Id = 2, MovieId = 2, CustomerId = 1, 
                    DaysRented = 3, PaymentMethod = "PayPal" }
             ]);

            DataContext.SaveChanges();
        }

        public void Dispose()
        {
            DataContext.Database.EnsureDeleted();
            DataContext?.Dispose();
        }
        public async Task DisposeAsync()
        {
            await DataContext.Database.EnsureDeletedAsync();
            DataContext?.Dispose();
        }
    }
}
