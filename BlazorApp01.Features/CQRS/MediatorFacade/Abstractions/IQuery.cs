using Ardalis.Result;
using Mediator;

namespace BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{ }
