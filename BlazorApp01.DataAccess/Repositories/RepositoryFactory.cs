using BlazorApp01.DataAccess.Contexts;
using BlazorApp01.Domain.Abstractions;

namespace BlazorApp01.DataAccess.Repositories;

public interface IRepositoryFactory
{
    IRepository<TEntity> Create<TEntity>(AppDbContext context) where TEntity : class, IEntity;
}

internal sealed class RepositoryFactory : IRepositoryFactory
{
    public IRepository<TEntity> Create<TEntity>(AppDbContext context) where TEntity : class, IEntity
    {
        return new Repository<TEntity>(context);
    }
}