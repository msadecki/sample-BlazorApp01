using BlazorApp01.DataAccess.Contexts;
using BlazorApp01.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BlazorApp01.DataAccess.Repositories;

public interface IQueryRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> FindAsNoTrackingAsync(object?[]? keyValues, CancellationToken cancellationToken);
    Task<TEntity?> FindAsNoTrackingAsync<TKeyValue>(TKeyValue keyValue, CancellationToken cancellationToken);
    Task<List<TEntity>> GetAllAsNoTrackingAsync(CancellationToken cancellationToken);
    IQueryable<TEntity> QueryAsNoTracking();
}

public interface ICommandRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken);
    Task<TEntity?> FindAsync<TKeyValue>(TKeyValue keyValue, CancellationToken cancellationToken);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    EntityEntry<TEntity> Update(TEntity entity);
    EntityEntry<TEntity> Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
    IQueryable<TEntity> Query();
}

public interface IRepository
{ }

public interface IRepository<TEntity> : IRepository, IQueryRepository<TEntity>, ICommandRepository<TEntity>
     where TEntity : class, IEntity
{ }

internal sealed class Repository<TEntity>(AppDbContext context) : IRepository<TEntity> where TEntity : class, IEntity
{
    private readonly AppDbContext _context = context;
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<TEntity?> FindAsNoTrackingAsync(object?[]? keyValues, CancellationToken cancellationToken)
    {
        var entity = await _dbSet.FindAsync(keyValues, cancellationToken);

        if (entity != null)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        return entity;
    }

    public async Task<TEntity?> FindAsNoTrackingAsync<TKeyValue>(TKeyValue keyValue, CancellationToken cancellationToken) =>
        await FindAsNoTrackingAsync([keyValue], cancellationToken);

    public async Task<List<TEntity>> GetAllAsNoTrackingAsync(CancellationToken cancellationToken) =>
        await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public IQueryable<TEntity> QueryAsNoTracking() =>
        _dbSet.AsNoTracking();

    public async Task<TEntity?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken) =>
        await _dbSet.FindAsync(keyValues, cancellationToken);

    public async Task<TEntity?> FindAsync<TKeyValue>(TKeyValue keyValue, CancellationToken cancellationToken) =>
        await FindAsync([keyValue], cancellationToken);

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken) =>
        (await _dbSet.AddAsync(entity, cancellationToken)).Entity;

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken) =>
        await _dbSet.AddRangeAsync(entities, cancellationToken);

    public EntityEntry<TEntity> Update(TEntity entity) =>
        _dbSet.Update(entity);

    public EntityEntry<TEntity> Remove(TEntity entity) =>
        _dbSet.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) =>
        _dbSet.RemoveRange(entities);

    public IQueryable<TEntity> Query() =>
        _dbSet;
}