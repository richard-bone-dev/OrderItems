namespace Api.Infrastructure.Http;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class ApiErrorResponseFactory : IApiErrorResponseFactory
{
    public ObjectResult Create(Exception exception, HttpContext httpContext)
    {
        if (exception is ValidationException validationException)
        {
            return CreateValidationResult(httpContext, new Dictionary<string, string[]>
            {
                [string.Empty] = [validationException.Message]
            });
        }

        var (status, title) = MapException(exception);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{status}"
        };

        AddCommonExtensions(problem, httpContext);

        return new ObjectResult(problem)
        {
            StatusCode = status
        };
    }

    public ObjectResult Create(ActionContext actionContext)
    {
        var errors = actionContext.ModelState
            .Where(kvp => kvp.Value is { Errors.Count: > 0 })
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                    ? "The input was not valid."
                    : e.ErrorMessage).ToArray());

        return CreateValidationResult(actionContext.HttpContext, errors);
    }

    private static ObjectResult CreateValidationResult(HttpContext httpContext, IDictionary<string, string[]> errors)
    {
        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = $"https://httpstatuses.com/{StatusCodes.Status400BadRequest}",
            Instance = httpContext.Request.Path
        };

        AddCommonExtensions(problem, httpContext);

        return new BadRequestObjectResult(problem);
    }

    private static (int Status, string Title) MapException(Exception exception)
        => exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Invalid Operation"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

    private static void AddCommonExtensions(ProblemDetails problem, HttpContext httpContext)
    {
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
    }
}
