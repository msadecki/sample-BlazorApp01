using BlazorApp01.BackgroundProcessing.Jobs;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp01.BackgroundProcessing;

public static class BackgroundProcessingRegistration
{
    public static void RegisterBackgroundProcessing(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(
                configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true
                });
        });

        services.AddHangfireServer();

        services.AddScoped<AddRandomCustomTaskJob>();
    }

    public static void AddCronJobs(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<AddRandomCustomTaskJob>(
            $"RecurringJob-{nameof(AddRandomCustomTaskJob)}",
            job => job.ExecuteAsync(),
            configuration.GetSection("CronJobs:AddRandomCustomTaskJob")?.Value ?? "*/5 * * * *"
        );
    }
}