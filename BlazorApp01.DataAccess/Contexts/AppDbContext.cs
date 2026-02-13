using BlazorApp01.Domain.Models;
using BlazorApp01.Domain.Models.EventStore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.DataAccess.Contexts;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<CustomTask> CustomTasks => Set<CustomTask>();
    public DbSet<StoredEvent> StoredEvents => Set<StoredEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>()
               .HaveColumnType("datetime2(0)");

        builder.Properties<DateTime?>()
               .HaveColumnType("datetime2(0)");
    }
}