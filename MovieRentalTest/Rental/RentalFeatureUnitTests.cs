using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRental.PaymentProviders;
using MovieRental.Rental;

namespace MovieRentalUnitTests.Rental
{
    public class RentalFeatureUnitTests : IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture _databaseFixture;
        private Mock<ILogger<RentalFeatures>> _mockLogger { get; } = new();
        private Mock<IPaymentProviderFactory> _mockPaymentProviderFactory { get; } = new();

        public RentalFeatureUnitTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }

        [Theory]
        [InlineData("MbWay")]
        [InlineData("PayPal")]
        public async Task SaveChangesAsync_PaymentIsTrue_SavesRentalAsync(string paymentMethod)
        {
            // Arrange
            _databaseFixture.Initialize();
            var rental = new MovieRental.Rental.Rental { 
                // Leave Id as 0 so EF will treat this as a new entity
                CustomerId = 1, MovieId = 1, 
                PaymentMethod = paymentMethod, DaysRented = 3 };

            var paymentProvider = paymentMethod switch
            {
                "MbWay" => new Mock<IPaymentProvider>(),
                "PayPal" => new Mock<IPaymentProvider>(),
                _ => throw new ArgumentException("Invalid payment method")
            };

            paymentProvider.Setup(m => m.Pay(It.IsAny<double>()))
                .ReturnsAsync(true);

            _mockPaymentProviderFactory.Setup(m => m.GetPaymentProvider(It.IsAny<string>()))
                .Returns(paymentProvider.Object);

            var sut = new RentalFeatures(_databaseFixture.DataContext, _mockPaymentProviderFactory.Object, _mockLogger.Object);

            // Act
            var response = await sut.SaveRentalAsync(rental);

            // Assert
            response.Should().NotBeNull();
            response.isSuccess.Should().BeTrue();
            response.GetData().Should().NotBeNull();

            var rentalsInDb = await _databaseFixture.DataContext.Rentals.ToListAsync();
            rentalsInDb.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetRentalsByCustomerNameAsync_ExistingCustomer_ReturnsRentals()
        {
            // Arrange
            _databaseFixture.Initialize();
            var sut = new RentalFeatures(_databaseFixture.DataContext, 
                _mockPaymentProviderFactory.Object, _mockLogger.Object);

            var customerName = "Jhon Doe";

            // Act
            var result = await sut.GetRentalsByCustomerNameAsync(customerName);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetRentalsByCustomerNameAsync_InexistingCustomer_ReturnsError()
        {
            // Arrange
            _databaseFixture.Initialize();
            var sut = new RentalFeatures(_databaseFixture.DataContext, 
                _mockPaymentProviderFactory.Object, _mockLogger.Object);

            var customerName = "Jane Doe";

            // Act
            var response = await sut.GetRentalsByCustomerNameAsync(customerName);

            // Assert
            response.Should().NotBeNull();
            response.isSuccess.Should().BeFalse();
            response.GetErrors()?.Any(a => a.Message == $"No rentals found for the customer '{customerName}'.");
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

            var sut = new RentalFeatures(_databaseFixture.DataContext, _mockPaymentProviderFactory.Object, _mockLogger.Object);

            // Act
            var result = await sut.SaveRentalAsync(rental);

            // Assert
            result.isSuccess.Should().BeFalse();
            result.GetErrors()?.Any(a => a.Message == $"No payment provider registered for PaymentMethod {rental.PaymentMethod}.");
        }
    }
}
