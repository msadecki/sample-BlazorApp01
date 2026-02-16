using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models.EventStore;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;

namespace BlazorApp01.Features.CQRS.Requests.StoredEvents.Queries;

public sealed record GetStoredEventByIdQuery(long StoredEventId) : IQuery<StoredEvent?>;

internal sealed class GetStoredEventByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetStoredEventByIdQuery, StoredEvent?>
{
    public async ValueTask<Result<StoredEvent?>> Handle(GetStoredEventByIdQuery request, CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<StoredEvent>()
            .FindAsNoTrackingAsync(request.StoredEventId, cancellationToken);
    }
}