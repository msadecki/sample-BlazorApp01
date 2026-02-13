using BlazorApp01.Features.Facade.CQRSMediator;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp01.Features;

public static class FeaturesRegistration
{
    public static IServiceCollection RegisterFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        AddMediator(services);

        return services;
    }

    private static void AddMediator(IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.Namespace = "BlazorApp01.Features.Mediator";
            options.ServiceLifetime = ServiceLifetime.Scoped;
            // Only available from v3:
            options.GenerateTypesAsInternal = true;
            options.NotificationPublisherType = typeof(ForeachAwaitPublisher);
            options.Assemblies = [typeof(FeaturesRegistration).Assembly];
            options.PipelineBehaviors = [];
            options.StreamPipelineBehaviors = [];
            // Only available from v3.1:
            //options..CachingMode = CachingMode.Eager;
        });

        services.AddScoped<ISenderFacade, SenderFacade>();
    }
}