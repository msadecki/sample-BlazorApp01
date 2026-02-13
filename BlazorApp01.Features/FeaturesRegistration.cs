using BlazorApp01.Features.CQRS.Behaviors;
using BlazorApp01.Features.CQRS.MediatorFacade;
using BlazorApp01.Features.Services.EventStore;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp01.Features;

public static class FeaturesRegistration
{
    public static IServiceCollection RegisterFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        AddValidation(services);
        AddMediator(services);
        AddEventSourcing(services);

        return services;
    }

    private static void AddValidation(IServiceCollection services)
    {
        // Register all validators from this assembly
        services.AddValidatorsFromAssembly(
            typeof(FeaturesRegistration).Assembly,
            includeInternalTypes: true);
    }

    private static void AddMediator(IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.Namespace = "BlazorApp01.Features.Mediator";
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.GenerateTypesAsInternal = true;
            options.NotificationPublisherType = typeof(ForeachAwaitPublisher);
            options.Assemblies = [typeof(FeaturesRegistration).Assembly];
            
            // 🔥 Register validation pipeline behavior
            options.PipelineBehaviors = 
            [
                typeof(ValidationBehavior<,>)
            ];
            
            options.StreamPipelineBehaviors = [];
        });

        services.AddScoped<ISenderFacade, SenderFacade>();
    }

    private static void AddEventSourcing(IServiceCollection services)
    {
        // Register JSON serializer options provider as singleton for performance
        services.AddSingleton<IJsonSerializerOptionsProvider, JsonSerializerOptionsProvider>();
        
        // Register event sourcing services
        services.AddScoped<IEventStoreService, EventStoreService>();
        services.AddScoped<IOutboxService, OutboxService>();
        services.AddScoped<IEventPublisher, EventPublisher>();
    }
}