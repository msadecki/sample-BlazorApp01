using Ardalis.Result;
using FluentValidation;
using Mediator;

namespace BlazorApp01.Features.CQRS.Behaviors;

/// <summary>
/// Pipeline behavior that validates requests using FluentValidation before they reach handlers.
/// Returns validation errors as Result.Invalid() to maintain consistency with Ardalis.Result pattern.
/// </summary>
internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators registered for this request type, skip validation
        if (!validators.Any())
        {
            return await next(request, cancellationToken);
        }

        // Run all validators
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        // If validation failed, return Result.Invalid with validation errors
        if (failures.Count != 0)
        {
            var validationErrors = failures
                .Select(failure => new ValidationError(
                    identifier: failure.PropertyName,
                    errorMessage: failure.ErrorMessage,
                    errorCode: failure.ErrorCode,
                    severity: ConvertSeverity(failure.Severity)))
                .ToList();

            return CreateInvalidResult<TResponse>(validationErrors);
        }

        // Validation passed, continue to handler
        return await next(request, cancellationToken);
    }

    private static ValidationSeverity ConvertSeverity(Severity severity)
    {
        return severity switch
        {
            Severity.Error => ValidationSeverity.Error,
            Severity.Warning => ValidationSeverity.Warning,
            Severity.Info => ValidationSeverity.Info,
            _ => ValidationSeverity.Error
        };
    }

    private static TResponse CreateInvalidResult<T>(List<ValidationError> validationErrors)
    {
        var resultType = typeof(T);

        // Handle Result<TValue>
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = resultType.GetGenericArguments()[0];
            var invalidMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod(nameof(Result.Invalid), new[] { typeof(List<ValidationError>) });

            return (TResponse)invalidMethod!.Invoke(null, new object[] { validationErrors })!;
        }

        // Handle Result (non-generic)
        if (resultType == typeof(Result))
        {
            return (TResponse)(object)Result.Invalid(validationErrors);
        }

        throw new InvalidOperationException(
            $"Unsupported result type: {resultType.Name}. " +
            "ValidationBehavior only supports Result and Result<T>.");
    }
}