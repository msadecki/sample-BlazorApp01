using BlazorApp01.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BlazorApp01.DataAccess.Contexts;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<CustomTask> CustomTasks { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>()
               .HaveColumnType("datetime2(0)");

        builder.Properties<DateTime?>()
               .HaveColumnType("datetime2(0)");
    }
}