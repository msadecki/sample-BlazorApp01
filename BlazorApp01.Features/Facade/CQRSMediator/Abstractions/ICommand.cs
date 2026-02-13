using Ardalis.Result;
using Mediator;

namespace BlazorApp01.Features.Facade.CQRSMediator.Abstractions;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{ }

public interface ICommand : IRequest<Result>
{ }