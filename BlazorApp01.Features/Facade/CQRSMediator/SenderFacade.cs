using Ardalis.Result;
using BlazorApp01.Features.Extensions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BlazorApp01.Features.Facade.CQRSMediator;

public interface ISenderFacade
{
    ValueTask<Result<TResponse>> SendAsync<TResponse>(Abstractions.ICommand<TResponse> request, CancellationToken cancellationToken = default);
    ValueTask<Result> SendAsync(Abstractions.ICommand request, CancellationToken cancellationToken = default);
    ValueTask<Result<TResponse>> SendAsync<TResponse>(Abstractions.IQuery<TResponse> request, CancellationToken cancellationToken = default);
}

internal sealed class SenderFacade(ISender sender, ILogger<SenderFacade> logger) : ISenderFacade
{
    public async ValueTask<Result<TResponse>> SendAsync<TResponse>(Abstractions.ICommand<TResponse> request, CancellationToken cancellationToken = default)
    {
        return await BaseSendAsync<Result<TResponse>, TResponse>(request, cancellationToken);
    }

    public async ValueTask<Result> SendAsync(Abstractions.ICommand request, CancellationToken cancellationToken = default)
    {
        return await BaseSendAsync<Result, Result>(request, cancellationToken);
    }

    public async ValueTask<Result<TResponse>> SendAsync<TResponse>(Abstractions.IQuery<TResponse> request, CancellationToken cancellationToken = default)
    {
        return await BaseSendAsync<Result<TResponse>, TResponse>(request, cancellationToken);
    }

    private async ValueTask<TResult> BaseSendAsync<TResult, TResponse>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        where TResult : Result<TResponse>
    {
        logger.LogInformation("Handling {1}", request.ToString());

        TResult result;
        try
        {
            result = await sender.Send(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception thrown while handling {1}: {2}", request.ToString(), ex.Message);
            return (TResult)Result.Error("Unexpected error occurred");
        }

        if (result.IsOk() || result.IsNoContent() || result.IsCreated())
        {
            logger.LogInformation("Handled {1}: Result status: {2}, Result type: {3}", request.ToString(), result.Status, result.GetValue()?.ToString());
        }
        else
        {
            logger.LogInformation("Handled {1}: Result status: {2}, Errors: {3}", request.ToString(), result.Status, result.MergedErrors());
        }

        return result;
    }
}