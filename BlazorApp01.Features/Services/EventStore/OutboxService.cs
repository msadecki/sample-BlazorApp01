using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Models.EventStore;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.Services.EventStore;

public interface IOutboxService
{
    Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default);
    Task MarkAsPublishedAsync(long outboxMessageId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(long outboxMessageId, string error, CancellationToken cancellationToken = default);
}

internal sealed class OutboxService(IUnitOfWork unitOfWork) : IOutboxService
{
    public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await unitOfWork.CommandRepository<OutboxMessage>().AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.QueryRepository<OutboxMessage>()
            .QueryAsNoTracking()
            .Where(outboxMessage => outboxMessage.Status == OutboxMessageStatus.Pending && outboxMessage.RetryCount < 5)
            .OrderBy(outboxMessage => outboxMessage.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsPublishedAsync(long outboxMessageId, CancellationToken cancellationToken = default)
    {
        var outboxMessage = await unitOfWork.CommandRepository<OutboxMessage>().FindAsync(outboxMessageId, cancellationToken);
        if (outboxMessage != null)
        {
            outboxMessage.Status = OutboxMessageStatus.Published;
            outboxMessage.PublishedAt = DateTime.UtcNow;
            unitOfWork.CommandRepository<OutboxMessage>().Update(outboxMessage);
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAsFailedAsync(long outboxMessageId, string error, CancellationToken cancellationToken = default)
    {
        var outboxMessage = await unitOfWork.CommandRepository<OutboxMessage>().FindAsync(outboxMessageId, cancellationToken);
        if (outboxMessage != null)
        {
            outboxMessage.Status = OutboxMessageStatus.Failed;
            outboxMessage.Error = error.Length > 2000 ? error[..2000] : error;
            outboxMessage.RetryCount++;
            outboxMessage.ProcessedAt = DateTime.UtcNow;
            unitOfWork.CommandRepository<OutboxMessage>().Update(outboxMessage);
        }

        await unitOfWork.SaveChangesAsync();
    }
}