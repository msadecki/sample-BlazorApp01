using BlazorApp01.BackgroundProcessing;
using BlazorApp01.DataAccess;
using BlazorApp01.Features;
using BlazorApp01.Messaging;
using BlazorApp01.Web;
using BlazorApp01.Web.Components;
using BlazorApp01.Web.Components.Account;
using Hangfire;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.RegisterDataAccess(builder.Configuration);
builder.Services.RegisterFeatures(builder.Configuration);
builder.Services.RegisterMessaging(builder.Configuration);
builder.Services.RegisterBackgroundProcessing(builder.Configuration);
builder.Services.RegisterWeb(builder.Configuration);

// Enable detailed circuit errors in Development to see full exception details in the browser
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<CircuitOptions>(options => options.DetailedErrors = true);
}

var app = builder.Build();

app.Services.MigrateAndSeedDatabase();

// Register cron jobs and add Hangfire dashboard (UI at {baseUrl}/hangfire).
app.Services.AddCronJobs();
app.UseHangfireDashboard();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMigrationsEndPoint();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization();

app.MapAdditionalIdentityEndpoints();

app.Run();