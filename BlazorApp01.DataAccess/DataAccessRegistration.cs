using BlazorApp01.DataAccess.Contexts;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorApp01.DataAccess;

public static class DataAccessRegistration
{
    private class Dummy();

    public static IServiceCollection RegisterDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }

    public static void MigrateAndSeedDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        try
        {
            var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var context = contextFactory.CreateDbContext();

            // Check if the database provider is relational before applying migrations.
            // This prevents the "Relational-specific methods" exception during integration tests using In-Memory DB.
            if (context.Database.IsRelational())
            {
                context.Database.Migrate();
            }

            SeedData.Initialize(context);
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Dummy>>();
            logger.LogError(ex, "An error occurred while migrating and seeding the database.");
        }
    }

    private static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // REMARKS: Add seeding logic here if needed.
            EnsureSeedCustomTasks(context);
        }

        private static void EnsureSeedCustomTasks(AppDbContext context)
        {
            if (context.CustomTasks.Any())
            {
                return;
            }

            var customTasks = new[]
            {
                new CustomTask
                {
                    Description = "Task 1",
                    Status = CustomTaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                    CompletionDate = null,
                    IsActive = true
                },
                new CustomTask
                {
                    Description = "Task 2",
                    Status = CustomTaskStatus.InProgress,
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
                    CompletionDate = null,
                    IsActive = true
                }
            };

            context.CustomTasks.AddRange(customTasks);
            context.SaveChanges();
        }
    }
}