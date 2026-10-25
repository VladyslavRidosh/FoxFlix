
using Foxflix.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Foxflix.Infrastructure.Persistence.Context;

public class FoxflixDbContextFactory
    : IDesignTimeDbContextFactory<FoxflixDbContext>
{
    public FoxflixDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<FoxflixDbContext>();

        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set ConnectionStrings__DefaultConnection before creating migrations.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new FoxflixDbContext(optionsBuilder.Options);
    }
}
