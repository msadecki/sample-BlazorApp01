using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models.EventStore;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.OutboxMessages.Queries;

public sealed record GetOutboxMessageByIdQuery(long OutboxMessageId) : IQuery<OutboxMessage?>;

internal sealed class GetOutboxMessageByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetOutboxMessageByIdQuery, OutboxMessage?>
{
    public async ValueTask<Result<OutboxMessage?>> Handle(GetOutboxMessageByIdQuery query, CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<OutboxMessage>()
            .QueryAsNoTracking()
            .FirstOrDefaultAsync(x => x.OutboxMessageId == query.OutboxMessageId, cancellationToken);
    }
}