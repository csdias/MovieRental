using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRental.Movie;

namespace MovieRentalUnitTests.Movie
{
    public class MovieFeatureUnitTests: IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture _databaseFixture;
        private Mock<ILogger<MovieFeatures>> _mockLogger = new();

        public MovieFeatureUnitTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }

        [Fact]
        public async Task SaveChangesAsync_ValidInput_SavesAsync()
        {
            // Arrange
            _databaseFixture.Initialize();
            var movieFour = new MovieRental.Movie.Movie { Id = 4, Title = "Movie Four" };

            var sut = new MovieFeatures(_databaseFixture.DataContext, _mockLogger.Object);

            // Act
            var result = await sut.SaveMovieAsync(movieFour);

            // Assert
            result.Should().BeEquivalentTo(movieFour);

            var moviesInDb = await _databaseFixture.DataContext.Movies.ToListAsync();
            moviesInDb.Should().HaveCount(4);

            await _databaseFixture.DisposeAsync();
        }
    }
}
