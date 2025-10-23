namespace Api.Tests.Infrastructure.Http;

using Api.Infrastructure.Http;
using Api.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class ExceptionMappingFilterTests
{
    [Fact]
    public async Task OnExceptionAsync_ShouldTranslateException_IntoProblemDetails()
    {
        var factory = new ApiErrorResponseFactory();
        var filter = new ExceptionMappingFilter(factory, NullLogger<ExceptionMappingFilter>.Instance);
        var httpContext = TestHttpContextFactory.Create("/api/orders/1");
        var exceptionContext = new ExceptionContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new KeyNotFoundException("Order not found")
        };

        await filter.OnExceptionAsync(exceptionContext);

        exceptionContext.ExceptionHandled.Should().BeTrue();
        exceptionContext.Result.Should().BeOfType<ObjectResult>().Which.Should().Satisfy(result =>
        {
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Value.Should().BeOfType<ProblemDetails>().Which.Title.Should().Be("Resource Not Found");
        });
    }

    [Fact]
    public async Task OnExceptionAsync_ShouldDefaultToServerError_ForUnexpectedException()
    {
        var factory = new ApiErrorResponseFactory();
        var filter = new ExceptionMappingFilter(factory, NullLogger<ExceptionMappingFilter>.Instance);
        var httpContext = TestHttpContextFactory.Create("/api/payments");
        var exceptionContext = new ExceptionContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new NullReferenceException("Something went terribly wrong")
        };

        await filter.OnExceptionAsync(exceptionContext);

        exceptionContext.ExceptionHandled.Should().BeTrue();
        exceptionContext.Result.Should().BeOfType<ObjectResult>().Which.Should().Satisfy(result =>
        {
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            result.Value.Should().BeOfType<ProblemDetails>().Which.Title.Should().Be("An unexpected error occurred");
        });
    }
}
