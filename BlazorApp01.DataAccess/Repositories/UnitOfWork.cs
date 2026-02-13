using System.Collections.Concurrent;
using BlazorApp01.DataAccess.Contexts;
using BlazorApp01.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.DataAccess.Repositories;

/// <summary>
/// Provides a unit of work pattern implementation for managing database transactions
/// and coordinating multiple repository operations.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets a generic repository instance for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that implements IEntity.</typeparam>
    /// <returns>A repository instance for the specified entity type.</returns>
    IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity;

    /// <summary>
    /// Asynchronously saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously saves all changes made in this unit of work to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    int SaveChanges();
}

internal sealed class UnitOfWork(
    IDbContextFactory<AppDbContext> contextFactory,
    IRepositoryFactory repositoryFactory) : IUnitOfWork
{
    private readonly AppDbContext _context = contextFactory.CreateDbContext();
    private readonly ConcurrentDictionary<Type, IRepository> _repositories = new ConcurrentDictionary<Type, IRepository>();
    private bool _disposed;

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
    {
        return (IRepository<TEntity>)_repositories.GetOrAdd(
            typeof(TEntity),
            _ => repositoryFactory.Create<TEntity>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            await _context.DisposeAsync();
        }
        _disposed = true;
    }
}