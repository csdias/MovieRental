using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRental.Rental;

namespace MovieRentalUnitTests.Rental
{
    public class RentalFeatureUnitTests : IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture _databaseFixture;
        private Mock<ILogger<RentalFeatures>> _mockLogger { get; } = new();
        private Mock<IServiceProvider> _mockServiceProvider { get; } = new();

        public RentalFeatureUnitTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }

        [Theory]
        [InlineData("MbWay")]
        [InlineData("PayPal")]
        public async Task SaveChangesAsync_ValidInput_SavesAsync(string paymentMethod)
        {
            // Arrange
            _databaseFixture.Initialize();
            var rental = new MovieRental.Rental.Rental { 
                Id = 1, CustomerId = 1, MovieId = 1, 
                PaymentMethod = paymentMethod, DaysRented = 3 };

            var sut = new RentalFeatures(_databaseFixture.DataContext, _mockServiceProvider.Object, _mockLogger.Object);

            // Act
            var result = await sut.SaveRentalAsync(rental);

            // Assert
            result.Should().Be(1);

            var moviesInDb = await _databaseFixture.DataContext.Movies.ToListAsync();
            moviesInDb.Should().HaveCount(4);

            await _databaseFixture.DisposeAsync();
        }

        [Fact]
        public async Task GetRentalsByCustomerNameAsync_ExistingCustomer_ReturnsRentals()
        {
            // Arrange
            _databaseFixture.Initialize();
            var sut = new RentalFeatures(_databaseFixture.DataContext, 
                _mockServiceProvider.Object, _mockLogger.Object);

            var customerName = "Jhon Doe";

            // Act
            var result = await sut.GetRentalsByCustomerNameAsync(customerName);

            // Assert
            result.Should().NotBeNull();

            _databaseFixture.Dispose();
        }

        [Fact]
        public async Task GetRentalsByCustomerNameAsync_InexistingCustomer_ReturnsError()
        {
            // Arrange
            _databaseFixture.Initialize();
            var sut = new RentalFeatures(_databaseFixture.DataContext, 
                _mockServiceProvider.Object, _mockLogger.Object);

            var customerName = "Jane Doe";

            // Act
            var response = await sut.GetRentalsByCustomerNameAsync(customerName);

            // Assert
            response.Should().NotBeNull();
            response.isSuccess.Should().BeFalse();
            response.GetErrors()?.Any(a => a.Message == $"No rentals found for the customer '{customerName}'.");

            _databaseFixture.Dispose();
        }

        [Fact]
        public async Task SaveChangesAsync_InvalidPaymentMethod_ReturnsError()
        {
            // Arrange
            _databaseFixture.Initialize();
            var rental = new MovieRental.Rental.Rental
            {
                Id = 1,
                CustomerId = 1,
                MovieId = 1,
                PaymentMethod = "Stripe",
                DaysRented = 3
            };

            var sut = new RentalFeatures(_databaseFixture.DataContext, _mockServiceProvider.Object, _mockLogger.Object);

            // Act
            var result = await sut.SaveRentalAsync(rental);

            // Assert
            result.isSuccess.Should().BeFalse();
            result.GetErrors()?.Any(a => a.Message == $"No payment provider registered for PaymentMethod {rental.PaymentMethod}.");


            await _databaseFixture.DisposeAsync();
        }
    }
}
