using Ardalis.Result;
using BlazorApp01.Features.Extensions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BlazorApp01.Features.CQRS.MediatorFacade;

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

    private async ValueTask<TResult> BaseSendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        where TResult : IResult
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

            result = (TResult)CreateErrorResult<TResult>("Unexpected error occurred.");

            return result;
        }

        if (result.IsOk() || result.IsNoContent() || result.IsCreated())
        {
            var resultValue = result.GetValue();

            var valueInfo = resultValue != null
                ? $", Result value type: {resultValue.GetType().Name}"
                : string.Empty;

            logger.LogInformation("Handled {1}: Result status: {2}{3}", request.ToString(), result.Status, valueInfo);
        }
        else
        {
            logger.LogInformation("Handled {1}: Result status: {2}, Errors: {3}", request.ToString(), result.Status, result.MergedErrors());
        }

        return result;
    }

    private static IResult CreateErrorResult<TResult>(string errorMessage) where TResult : IResult
    {
        var resultType = typeof(TResult);

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var responseType = resultType.GetGenericArguments()[0];
            var errorMethod = typeof(Result<>).MakeGenericType(responseType).GetMethod("Error", [typeof(string)]);
            return (IResult)errorMethod!.Invoke(null, [errorMessage])!;
        }

        return Result.Error(errorMessage);
    }
}