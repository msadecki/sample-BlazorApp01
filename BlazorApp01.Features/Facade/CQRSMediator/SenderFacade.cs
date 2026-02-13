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
        return await BaseSendAsync(request, cancellationToken);
    }

    public async ValueTask<Result> SendAsync(Abstractions.ICommand request, CancellationToken cancellationToken = default)
    {
        return await BaseSendAsync(request, cancellationToken);
    }

    public async ValueTask<Result<TResponse>> SendAsync<TResponse>(Abstractions.IQuery<TResponse> request, CancellationToken cancellationToken = default)
    {
        return await BaseSendAsync(request, cancellationToken);
    }

    private async ValueTask<TResponse> BaseSendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        where TResponse : IResult
    {
        logger.LogInformation("Handling {1}", request.ToString());

        IResult result;
        try
        {
            result = await sender.Send(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception thrown while handling {1}: {2}", request.ToString(), ex.Message);
            return GetUnexpectedErrorResult<TResponse>();
        }

        if (result.IsOk() || result.IsNoContent() || result.IsCreated())
        {
            logger.LogInformation("Handled {1}: Result status: {2}, Result type: {3}", request.ToString(), result.Status, result.GetValue()?.ToString());
        }
        else
        {
            logger.LogInformation("Handled {1}: Result status: {2}, Errors: {3}", request.ToString(), result.Status, result.MergedErrors());
        }

        return (TResponse)result;
    }

    private TResponse GetUnexpectedErrorResult<TResponse>()
        where TResponse : IResult
    {
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Create Result<T>.Error() using reflection
            var valueType = responseType.GetGenericArguments()[0];
            var errorMethod = typeof(Result<>).MakeGenericType(valueType)
                .GetMethod(nameof(Result.Error), [typeof(string)]);
            return (TResponse)errorMethod!.Invoke(null, ["Unexpected error occurred"])!;
        }
        else
        {
            // Return non-generic Result.Error()
            return (TResponse)(IResult)Result.Error("Unexpected error occurred");
        }
    }
}