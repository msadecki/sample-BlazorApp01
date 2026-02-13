using Ardalis.Result;
using Mediator;

namespace BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{ }