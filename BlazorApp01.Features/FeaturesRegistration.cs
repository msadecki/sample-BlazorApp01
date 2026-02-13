using BlazorApp01.Features.CQRS.Behaviors;
using BlazorApp01.Features.CQRS.MediatorFacade;
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
}