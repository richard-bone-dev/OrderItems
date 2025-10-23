namespace Api.Infrastructure.Http;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

public class ExceptionMappingFilter : IAsyncExceptionFilter
{
    private readonly IApiErrorResponseFactory _responseFactory;
    private readonly ILogger<ExceptionMappingFilter> _logger;

    public ExceptionMappingFilter(IApiErrorResponseFactory responseFactory, ILogger<ExceptionMappingFilter> logger)
    {
        _responseFactory = responseFactory;
        _logger = logger;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.ExceptionHandled)
        {
            return Task.CompletedTask;
        }

        _logger.LogError(context.Exception, "Unhandled exception was translated to standardized response");

        context.Result = _responseFactory.Create(context.Exception, context.HttpContext);
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
