using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models.EventStore;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.StoredEvents.Queries;

public sealed record GetStoredEventByIdQuery(long StoredEventId) : IQuery<StoredEvent?>;

internal sealed class GetStoredEventByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetStoredEventByIdQuery, StoredEvent?>
{
    public async ValueTask<Result<StoredEvent?>> Handle(GetStoredEventByIdQuery query, CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<StoredEvent>()
            .QueryAsNoTracking()
            .FirstOrDefaultAsync(x => x.StoredEventId == query.StoredEventId, cancellationToken);
    }
}