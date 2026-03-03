using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Exceptions;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior to catch unhandled exceptions and return Result.Failure for handlers that return Result/Result{T}.
/// ValidationException is returned as 400 with specific error messages; other exceptions as InternalError.
/// </summary>
public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var responseType = typeof(TResponse);

            // Return validation errors to client (400) instead of generic InternalError
            if (ex is FluentValidation.ValidationException fluentEx)
            {
                var errors = fluentEx.Errors.Select(e => e.ErrorMessage).ToList();
                var message = errors.Count == 1 ? errors[0] : "Validation failed";
                return BuildValidationFailureResponse(responseType, message, errors);
            }

            if (ex is CapstoneProject.Application.Common.Exceptions.ValidationException appEx)
            {
                var errors = appEx.ErrorMessages;
                var message = errors.Count == 1 ? errors[0] : "Validation failed";
                return BuildValidationFailureResponse(responseType, message, errors);
            }

            _logger.LogError(ex, "Unhandled exception in {RequestName}: {Message}", typeof(TRequest).Name, ex.Message);

            if (responseType == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(
                    "An unexpected error occurred while processing your request. Please try again later.",
                    ErrorCodeEnum.InternalError);
            }

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var dataType = responseType.GetGenericArguments()[0];
                var resultType = typeof(Result<>).MakeGenericType(dataType);
                var failureMethod = resultType.GetMethod(nameof(Result<object>.Failure), new[] { typeof(string), typeof(ErrorCodeEnum) });
                if (failureMethod != null)
                {
                    var result = failureMethod.Invoke(null, new object[]
                    {
                        "An unexpected error occurred while processing your request. Please try again later.",
                        ErrorCodeEnum.InternalError
                    });
                    if (result != null)
                        return (TResponse)result;
                }
            }

            throw;
        }
    }

    private static TResponse BuildValidationFailureResponse(Type responseType, string message, List<string> errors)
    {
        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(message, ErrorCodeEnum.ValidationFailed, errors);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var dataType = responseType.GetGenericArguments()[0];
            var resultType = typeof(Result<>).MakeGenericType(dataType);
            var failureMethod = resultType.GetMethod(nameof(Result<object>.Failure), new[] { typeof(string), typeof(ErrorCodeEnum), typeof(List<string>) });
            if (failureMethod != null)
            {
                var result = failureMethod.Invoke(null, new object[] { message, ErrorCodeEnum.ValidationFailed, errors });
                if (result != null)
                    return (TResponse)result;
            }
        }

        throw new InvalidOperationException("Unsupported response type for validation failure.");
    }
}
