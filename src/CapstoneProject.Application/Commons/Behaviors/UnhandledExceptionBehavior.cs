using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior to catch unhandled exceptions and return Result.Failure for handlers that return Result/Result{T}.
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
            _logger.LogError(ex, "Unhandled exception in {RequestName}: {Message}", typeof(TRequest).Name, ex.Message);

            var responseType = typeof(TResponse);
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
}
