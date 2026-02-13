using BlazorApp01.DataAccess.Contexts;
using BlazorApp01.Domain.Abstractions;
using BlazorApp01.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.DataAccess.Repositories;

// KH: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
// Add repositories for other entities as needed
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IRepository<CustomTask> CustomTasksRepository { get; }

    IRepository<ApplicationUser> ApplicationUsersRepository { get; }

    IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}

internal class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories;
    private bool _disposed;

    private IRepository<CustomTask>? _customTasksRepository;

    private IRepository<ApplicationUser>? _applicationUsersRepository;

    public UnitOfWork(IDbContextFactory<AppDbContext> contextFactory)
    {
        _context = contextFactory.CreateDbContext();
        _repositories = new Dictionary<Type, object>();
    }

    public IRepository<CustomTask> CustomTasksRepository
    {
        get
        {
            _customTasksRepository ??= Repository<CustomTask>();
            return _customTasksRepository;
        }
    }

    public IRepository<ApplicationUser> ApplicationUsersRepository
    {
        get
        {
            _applicationUsersRepository ??= Repository<ApplicationUser>();
            return _applicationUsersRepository;
        }
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
    {
        var type = typeof(TEntity);
        
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<TEntity>(_context);
        }
        
        return (IRepository<TEntity>)_repositories[type];
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

    protected virtual void Dispose(bool disposing)
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

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            await _context.DisposeAsync();
        }
        _disposed = true;
    }
}