# BlazorApp01

# Sample source - Build a Blazor movie database app (Overview)

https://learn.microsoft.com/en-us/aspnet/core/blazor/tutorials/movie-database-app/?view=aspnetcore-10.0

# How to add a new EF migration

## Prerequisites - Ensure EF Core Tools are installed
```
PowerShell
Install-Package Microsoft.EntityFrameworkCore.Tools
```
Do this in BlazorWebAppMovies (future improvements - in both BlazorApp01.DataAccess & BlazorApp01.Web) if needed.

## Steps in Visual Studio

### Set the Startup Project
In Solution Explorer, right-click BlazorApp01.Web and choose Set as Startup Project.
This ensures EF Core uses the correct configuration and Program.cs for dependency injection.

### Open the Package Manager Console (PMC)
Go to Tools → NuGet Package Manager → Package Manager Console.

### Prepare some change to be updated
Add new domain model class (consider : IEntity) and new DbSet to AppDbContext or change existing domain model class.

### Run the Migration Command in PMC
```
PowerShell
Add-Migration AddSomeTable -Project BlazorApp01.DataAccess -StartupProject BlazorApp01.Web
```
Explanation:
- Add-Migration AddSomeTable → Creates a migration named AddSomeTable.
- -Project BlazorApp01.DataAccess → The project where your DbContext and migrations live.
- -StartupProject BlazorApp01.Web → The project that provides configuration and services.

## Steps in Docker Desktop Terminal

### Run RabbitMQ with management UI
```
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:management
```
or with a name for easier reference:
```
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
```

# Blazor Identity scaffolding has completed successfully.

## For setup and configuration information, see https://go.microsoft.com/fwlink/?linkid=2290075.

If the project had identity support prior to scaffolding, ensure that the following changes are present in Program.cs:
1. Correct DbContextClass is used in the following statements :
    - var connectionString = builder.Configuration.GetConnectionString(DBCONTEXT_CONNECTIONSTRING) ...
    - builder.Services.AddDbContext<DBCONTEXT>(options => ...
    - builder.Services.AddIdentityCore<IDENTITY_USER_CLASS>...
        .AddEntityFrameworkStores<DBCONTEXT>() ...
2. Correct Identity User class is being used, (if using the default 'IdentityUser' or a custom IdentityUser class).
    - builder.Services.AddIdentityCore<IDENTITY_USER_CLASS>...
    - builder.Services.AddSingleton<IEmailSender<IDENTITY_USER_CLASS>...
