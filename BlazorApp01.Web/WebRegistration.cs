using BlazorApp01.DataAccess.Contexts;
using BlazorApp01.Domain.Models;
using BlazorApp01.Web.Components.Account;
using BlazorApp01.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp01.Web;

public static class WebRegistration
{
    public static IServiceCollection RegisterWeb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddScoped<IDateTimeHelper, DateTimeHelper>();

        services.AddQuickGridEntityFrameworkAdapter();

        services.AddCascadingAuthenticationState();

        services.AddScoped<IdentityRedirectManager>();

        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddIdentityCookies();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        services.AddAuthorization();

        services.AddCascadingAuthenticationState();

        return services;
    }
}