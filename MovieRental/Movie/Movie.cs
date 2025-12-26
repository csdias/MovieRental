using System.ComponentModel.DataAnnotations;

namespace MovieRental.Movie
{
	public class Movie
	{
		[Key]
		public int Id { get; set; }
		public required string Title { get; set; } = string.Empty;
	}
}
