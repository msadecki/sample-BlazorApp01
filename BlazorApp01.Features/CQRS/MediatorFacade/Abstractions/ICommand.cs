using Ardalis.Result;
using Mediator;

namespace BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{ }

public interface ICommand : IRequest<Result>
{ }