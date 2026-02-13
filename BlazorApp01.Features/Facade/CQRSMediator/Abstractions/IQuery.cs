using Ardalis.Result;
using Mediator;

namespace BlazorApp01.Features.Facade.CQRSMediator.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{ }
