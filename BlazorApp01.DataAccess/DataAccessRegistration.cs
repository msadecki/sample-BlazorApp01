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

            var random = new Random(42); // Fixed seed for consistent data
            var customTasks = new List<CustomTask>();
            var baseDate = DateTime.UtcNow;

            var taskDescriptions = new[]
            {
                "Review code changes", "Update documentation", "Fix bug in login", "Implement new feature",
                "Refactor database queries", "Write unit tests", "Deploy to staging", "Update dependencies",
                "Review pull requests", "Optimize performance", "Create API endpoints", "Design UI mockups",
                "Configure CI/CD pipeline", "Update user manual", "Analyze system logs", "Backup database",
                "Security audit", "Client meeting preparation", "Sprint planning", "Code review session",
                "Update project roadmap", "Research new technologies", "Implement authentication", "Add validation rules",
                "Create migration scripts", "Test new features", "Monitor application health", "Update API documentation",
                "Refactor legacy code", "Implement caching strategy", "Configure load balancer", "Database optimization",
                "Write integration tests", "Update error handling", "Implement logging", "Design database schema",
                "Setup monitoring alerts", "Create deployment scripts", "Review security policies", "Update coding standards"
            };

            var statuses = Enum.GetValues<CustomTaskStatus>();

            for (int i = 1; i <= 100; i++)
            {
                var status = statuses[random.Next(statuses.Length)];
                var daysOffset = random.Next(-30, 60); // Tasks from 30 days ago to 60 days in the future
                var createdDaysAgo = random.Next(1, 90);
                var createdDate = baseDate.AddDays(-createdDaysAgo);
                
                var description = i <= taskDescriptions.Length 
                    ? taskDescriptions[i - 1] 
                    : $"{taskDescriptions[random.Next(taskDescriptions.Length)]} #{i}";

                DateTime? completionDate = null;
                if (status == CustomTaskStatus.Completed)
                {
                    // Completion date must be between creation date and now
                    var daysAfterCreation = random.Next(0, Math.Max(1, (int)(baseDate - createdDate).TotalDays + 1));
                    completionDate = createdDate.AddDays(daysAfterCreation);
                }

                var task = new CustomTask
                {
                    Description = description,
                    Status = status,
                    CreatedAt = createdDate,
                    DueDate = DateOnly.FromDateTime(baseDate.AddDays(daysOffset)),
                    CompletionDate = completionDate,
                    IsActive = random.Next(100) < 95 // 95% active, 5% inactive
                };

                customTasks.Add(task);
            }

            context.CustomTasks.AddRange(customTasks);
            context.SaveChanges();
        }
    }
}