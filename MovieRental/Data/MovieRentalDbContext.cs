using Microsoft.EntityFrameworkCore;

namespace MovieRental.Data
{
	public class MovieRentalDbContext : DbContext
	{
		public DbSet<Movie.Movie> Movies { get; set; }
		public DbSet<Rental.Rental> Rentals { get; set; }
		public DbSet<Customer.Customer> Customers { get; set; }

        protected string DbPath { get; }

		public MovieRentalDbContext(DbContextOptions<MovieRentalDbContext> options) : base(options)
		{
			var folder = Environment.SpecialFolder.LocalApplicationData;
			var path = Environment.GetFolderPath(folder);
			DbPath = Path.Join(path, "movierental.db");
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			if (!options.IsConfigured)
			{
				options.UseSqlite($"Data Source={DbPath}");
			}
			base.OnConfiguring(options);
		}
	}
}
