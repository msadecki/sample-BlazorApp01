using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Models.EventStore;

namespace BlazorApp01.Features.Services.EventStore;

public interface IOutboxService
{
    Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default);
    Task MarkAsPublishedAsync(long id, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(long id, string error, CancellationToken cancellationToken = default);
}

internal sealed class OutboxService(IUnitOfWork unitOfWork) : IOutboxService
{
    public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await unitOfWork.Repository<OutboxMessage>().AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<OutboxMessage>();
        var messages = repository.QueryAsNoTracking()
            .Where(m => m.Status == OutboxMessageStatus.Pending && m.RetryCount < 5)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToList();

        return messages;
    }

    public async Task MarkAsPublishedAsync(long id, CancellationToken cancellationToken = default)
    {
        var message = await unitOfWork.Repository<OutboxMessage>().GetByIdAsync(id, cancellationToken);
        if (message != null)
        {
            message.Status = OutboxMessageStatus.Published;
            message.PublishedAt = DateTime.UtcNow;
            unitOfWork.Repository<OutboxMessage>().Update(message);
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAsFailedAsync(long id, string error, CancellationToken cancellationToken = default)
    {
        var message = await unitOfWork.Repository<OutboxMessage>().GetByIdAsync(id, cancellationToken);
        if (message != null)
        {
            message.Status = OutboxMessageStatus.Failed;
            message.Error = error.Length > 2000 ? error[..2000] : error;
            message.RetryCount++;
            message.ProcessedAt = DateTime.UtcNow;
            unitOfWork.Repository<OutboxMessage>().Update(message);
        }

        await unitOfWork.SaveChangesAsync();
    }
}