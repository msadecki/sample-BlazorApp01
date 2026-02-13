using Ardalis.Result;

namespace BlazorApp01.Features.Extensions;

public static class ArdalisResultExtensions
{
    public static string MergedErrors(this IResult result)
    {
        var errors = string.Join(", ", result.Errors);
        var validationErrors = string.Join(", ", result.ValidationErrors.Select(e => e.ErrorMessage));
        return string.Join(", ", new[] { errors, validationErrors }.Where(s => !string.IsNullOrEmpty(s)));
    }

    public static async Task<Result> BindNoDataAsync(
        this Task<Result> resultTask,
        Func<Task<Result>> next)
    {
        var result = await resultTask;
        return result.IsSuccess ? await next() : result;
    }

    public static Task<Result<T>> AsTask<T>(this Result<T> result)
    {
        return Task.FromResult(result);
    }
}
