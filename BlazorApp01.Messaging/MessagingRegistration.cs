using BlazorApp01.Messaging.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BlazorApp01.Messaging;

public static class MessagingRegistration
{
    /// <summary>
    /// Register MassTransit with RabbitMQ
    /// </summary>
    public static IServiceCollection RegisterMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind RabbitMQ settings
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddMassTransit(busConfigurator =>
        {
            // Register all consumers from this assembly
            busConfigurator.AddConsumers(typeof(MessagingRegistration).Assembly);

            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                var rabbitMqSettings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                configurator.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
                {
                    h.Username(rabbitMqSettings.Username);
                    h.Password(rabbitMqSettings.Password);
                });

                // Configure message retry policy
                configurator.UseMessageRetry(r => r.Incremental(
                    retryLimit: rabbitMqSettings.RetryLimit,
                    initialInterval: TimeSpan.FromSeconds(rabbitMqSettings.RetryInitialIntervalSeconds),
                    intervalIncrement: TimeSpan.FromSeconds(rabbitMqSettings.RetryIntervalIncrementSeconds)));

                // Configure circuit breaker
                configurator.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                    cb.TripThreshold = 15;
                    cb.ActiveThreshold = 10;
                    cb.ResetInterval = TimeSpan.FromMinutes(5);
                });

                // Configure rate limiter
                configurator.UseRateLimit(1000, TimeSpan.FromSeconds(1));

                // Configure endpoints
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}