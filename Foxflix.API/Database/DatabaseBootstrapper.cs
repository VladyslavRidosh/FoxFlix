using System.Reflection;
using Foxflix.Infrastructure.Identity;
using Foxflix.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Database;

/// <summary>
/// Creates missing PostgreSQL tables without dropping or replacing existing data.
/// The SQL scripts are embedded in the API assembly so this also works after publish.
/// </summary>
public static class DatabaseBootstrapper
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var identityDb = scope.ServiceProvider.GetRequiredService<FoxflixIdentityDbContext>();
        var domainDb = scope.ServiceProvider.GetRequiredService<FoxflixDbContext>();

        await identityDb.Database.ExecuteSqlRawAsync(
            ReadEmbeddedSql("postgresql-identity-schema.sql"), cancellationToken);

        await domainDb.Database.ExecuteSqlRawAsync(
            ReadEmbeddedSql("postgresql-domain-schema.sql"), cancellationToken);
    }

    private static string ReadEmbeddedSql(string fileName)
    {
        var assembly = typeof(DatabaseBootstrapper).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException(
                $"Database schema resource '{fileName}' was not embedded in the API assembly.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Cannot open database schema resource '{resourceName}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
