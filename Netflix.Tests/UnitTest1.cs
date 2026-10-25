using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Netflix.Tests
{
    public class FavoriteTests
    {
        [Fact]
        public async Task Favorite_ShouldBeSavedWithMovie()
        {
            var options = new DbContextOptionsBuilder<FoxflixDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new FoxflixDbContext(options);

            var movie = new Movie
            {
                Title = "The Last Horizon",
                Description = "Test movie",
                ReleaseYear = 2026,
                Rating = 8.5,
                DurationMinutes = 120,
                Genre = "Adventure",
                Country = "USA",
                AgeRating = 13,
                PosterUrl = "/images/poster.jpg",
                BackdropUrl = "/images/backdrop.jpg"
            };

            context.Movies.Add(movie);
            await context.SaveChangesAsync();

            var favorite = new Favorite
            {
                UserId = "test-user-1",
                MovieId = movie.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Favorites.Add(favorite);
            await context.SaveChangesAsync();

            var savedFavorite = await context.Favorites
                .Include(f => f.Movie)
                .SingleAsync(f => f.UserId == "test-user-1");

            Assert.Equal(movie.Id, savedFavorite.MovieId);
            Assert.Equal("test-user-1", savedFavorite.UserId);
            Assert.Equal("The Last Horizon", savedFavorite.Movie.Title);
        }
    }


}
