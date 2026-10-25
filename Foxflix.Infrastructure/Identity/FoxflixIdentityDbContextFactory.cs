
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Foxflix.Infrastructure.Identity;

public class FoxflixIdentityDbContextFactory
    : IDesignTimeDbContextFactory<FoxflixIdentityDbContext>
{
    public FoxflixIdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set ConnectionStrings__DefaultConnection before creating Identity migrations.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<FoxflixIdentityDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new FoxflixIdentityDbContext(optionsBuilder.Options);
    }
}
