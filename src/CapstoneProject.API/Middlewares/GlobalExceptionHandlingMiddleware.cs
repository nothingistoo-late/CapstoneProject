using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Exceptions;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.API.Middlewares;

/// <summary>
/// Middleware for handling exceptions globally
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    
    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {

            //chỉ có các log nào thuộc về internal error, database error, external service error mới được log chi tiết ra file hoặc console
            if (ex is not ValidationException && ex is not FluentValidation.ValidationException && ex is not ArgumentException 
            && ex is not UnauthorizedAccessException && ex is not ForbiddenAccessException && ex is not KeyNotFoundException)
            {
                 _logger.LogError(ex, 
                "Unhandled exception occurred. RequestPath: {RequestPath}, Method: {Method}, User: {User}, " +
                "Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                context.Request.Path, 
                context.Request.Method,
                context.User?.Identity?.Name ?? "Anonymous",
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            }
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message, errors) = ClassifyException(exception);
        
        context.Response.StatusCode = (int)statusCode;
        var result = Result.Failure(message, errorCode, errors);
        context.Response.ContentType = "application/json";
        
        return context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
    
    private (HttpStatusCode statusCode, ErrorCodeEnum errorCode, string message, List<string>? errors) ClassifyException(Exception exception)
    {
        return exception switch
        {
            FluentValidation.ValidationException fluentValidationEx => (
                HttpStatusCode.BadRequest,
                ErrorCodeEnum.ValidationFailed,
                "Validation failed",
                fluentValidationEx.Errors.Select(e => e.ErrorMessage).ToList()
            ),
            
            CapstoneProject.Application.Common.Exceptions.ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ErrorCodeEnum.ValidationFailed,
                "Validation failed",
                validationEx.Errors.SelectMany(e => e.Value).ToList()
            ),
            
            ArgumentException => (
                HttpStatusCode.BadRequest,
                ErrorCodeEnum.InvalidInput,
                exception.Message ?? "Invalid input",
                null
            ),
            
            UnauthorizedAccessException unauthorizedEx when IsFileAccess(unauthorizedEx) => (
                HttpStatusCode.Forbidden,
                ErrorCodeEnum.StorageError,
                exception.Message ?? "File access denied",
                null
            ),
            
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ErrorCodeEnum.Unauthorized,
                exception.Message ?? "Authentication required",
                null
            ),
            
            ForbiddenAccessException => (
                HttpStatusCode.Forbidden,
                ErrorCodeEnum.Forbidden,
                exception.Message ?? "Access denied",
                null
            ),
            
            KeyNotFoundException keyNotFoundEx => (
                HttpStatusCode.NotFound,
                ErrorCodeEnum.NotFound,
                keyNotFoundEx.Message ?? "Resource not found",
                null
            ),
            
            InvalidOperationException invalidEx when IsDatabaseRelated(invalidEx) => HandleDatabaseException(invalidEx),
            
            InvalidOperationException invalidEx => (
                HttpStatusCode.BadRequest,
                ErrorCodeEnum.InvalidOperation,
                invalidEx.Message ?? "Invalid operation",
                null
            ),
            
            PostgresException pgEx => HandlePostgresException(pgEx),
            DbUpdateException dbEx => HandleDatabaseException(dbEx),
            
            TimeoutException => (
                HttpStatusCode.RequestTimeout,
                ErrorCodeEnum.ExternalServiceError,
                "Request timeout - please try again later",
                null
            ),
            
            FileNotFoundException => (
                HttpStatusCode.NotFound,
                ErrorCodeEnum.FileNotFound,
                "File not found",
                null
            ),
            
            DirectoryNotFoundException => (
                HttpStatusCode.NotFound,
                ErrorCodeEnum.FileNotFound,
                "Directory not found", 
                null
            ),
            
            HttpRequestException => (
                HttpStatusCode.BadGateway,
                ErrorCodeEnum.ExternalServiceError,
                "External service unavailable",
                null
            ),
            
            _ => (
                HttpStatusCode.InternalServerError,
                ErrorCodeEnum.InternalError,
                "An internal server error occurred",
                null
            )
        };
    }
    
    private (HttpStatusCode, ErrorCodeEnum, string, List<string>?) HandlePostgresException(PostgresException pgEx)
    {
        // https://www.postgresql.org/docs/current/errcodes-appendix.html
        return pgEx.SqlState switch
        {
            "08000" or "08003" or "08006" or "08001" or "57P01" or "57P02" or "57P03" => (
                HttpStatusCode.ServiceUnavailable,
                ErrorCodeEnum.DatabaseError,
                "Database service temporarily unavailable",
                null
            ),

            "28P01" or "28000" => (
                HttpStatusCode.ServiceUnavailable,
                ErrorCodeEnum.DatabaseError,
                "Database authentication failed",
                null
            ),

            "23503" or "23514" or "23502" => (
                HttpStatusCode.Conflict,
                ErrorCodeEnum.ResourceConflict,
                "Operation violates data constraints",
                null
            ),

            "23505" => (
                HttpStatusCode.Conflict,
                ErrorCodeEnum.DuplicateEntry,
                "Duplicate entry found",
                null
            ),

            "57014" or "55P03" => (
                HttpStatusCode.RequestTimeout,
                ErrorCodeEnum.DatabaseError,
                "Database request timeout",
                null
            ),

            "53300" => (
                HttpStatusCode.ServiceUnavailable,
                ErrorCodeEnum.DatabaseError,
                "Database service temporarily unavailable",
                null
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ErrorCodeEnum.DatabaseError,
                "Database operation failed",
                null
            )
        };
    }

    private (HttpStatusCode, ErrorCodeEnum, string, List<string>?) HandleDatabaseException(Exception exception)
    {
        return exception switch
        {
            DbUpdateException => (
                HttpStatusCode.Conflict,
                ErrorCodeEnum.ResourceConflict,
                "Data update conflict occurred",
                null
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ErrorCodeEnum.DatabaseError,
                "Database error occurred",
                null
            )
        };
    }
    
    private bool IsDatabaseRelated(Exception exception)
    {
        var message = exception.Message.ToLowerInvariant();
        return message.Contains("database") || 
               message.Contains("connection") || 
               message.Contains("sql") ||
               message.Contains("entity framework") ||
               message.Contains("dbcontext");
    }
    
    private bool IsFileAccess(Exception exception)
    {
        return exception.Message.Contains("file") || 
               exception.Message.Contains("directory") ||
               exception.Message.Contains("path");
    }
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
} 