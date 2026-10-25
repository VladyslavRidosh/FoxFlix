using Foxflix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.Infrastructure.Persistence.Context;

public class FoxflixDbContext : DbContext
{
    public FoxflixDbContext(DbContextOptions<FoxflixDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<Series> Series => Set<Series>();

    public DbSet<Season> Seasons => Set<Season>();

    public DbSet<Episode> Episodes => Set<Episode>();

    public DbSet<WatchHistory> WatchHistories => Set<WatchHistory>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<TmdbFavorite> TmdbFavorites => Set<TmdbFavorite>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        /*
         * SEASON
         */

        modelBuilder.Entity<Season>(entity =>
        {
            entity.HasOne(s => s.Series)
                .WithMany(s => s.Seasons)
                .HasForeignKey(s => s.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(s => new
            {
                s.SeriesId,
                s.SeasonNumber
            })
            .IsUnique();
        });


        /*
         * EPISODE
         */

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.HasOne(e => e.Season)
                .WithMany()
                .HasForeignKey(e => e.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new
            {
                e.SeasonId,
                e.EpisodeNumber
            })
            .IsUnique();
        });


        /*
         * WATCH HISTORY
         */

        modelBuilder.Entity<WatchHistory>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.UserId,
                x.MovieId
            })
            .IsUnique();

            entity.HasOne(x => x.Movie)
                .WithMany(x => x.WatchHistories)
                .HasForeignKey(x => x.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        /*
         * FAVORITES
         */

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.UserId,
                x.MovieId
            })
            .IsUnique();

            entity.HasOne(x => x.Movie)
                .WithMany()
                .HasForeignKey(x => x.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}