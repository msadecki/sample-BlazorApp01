using BlazorApp01.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.DataAccess.Contexts;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CustomTask> CustomTasks { get; set; }
}