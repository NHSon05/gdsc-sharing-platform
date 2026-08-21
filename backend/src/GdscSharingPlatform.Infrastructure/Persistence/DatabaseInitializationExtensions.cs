using GdscSharingPlatform.Infrastructure.Identity.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GdscSharingPlatform.Infrastructure.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope =
            services.CreateAsyncScope();

        var serviceProvider =
            scope.ServiceProvider;

        var dbContext =
            serviceProvider.GetRequiredService<
                ApplicationDbContext>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(
                cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(
                cancellationToken);
        }

        var databaseSeeder =
            serviceProvider.GetRequiredService<
                DatabaseSeeder>();

        await databaseSeeder.SeedAsync(
            cancellationToken);
    }
}