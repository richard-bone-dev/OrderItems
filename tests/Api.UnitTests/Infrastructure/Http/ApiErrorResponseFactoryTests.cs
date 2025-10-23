namespace Api.Tests.Infrastructure.Http;

using System.ComponentModel.DataAnnotations;
using Api.Infrastructure.Http;
using Api.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Xunit;

public class ApiErrorResponseFactoryTests
{
    private readonly ApiErrorResponseFactory _factory = new();

    [Theory]
    [InlineData(typeof(KeyNotFoundException), StatusCodes.Status404NotFound, "Resource Not Found")]
    [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized, "Unauthorized")]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest, "Invalid Request")]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status409Conflict, "Invalid Operation")]
    [InlineData(typeof(NullReferenceException), StatusCodes.Status500InternalServerError, "An unexpected error occurred")]
    public void Create_ShouldMapKnownExceptions_ToExpectedStatusAndTitle(Type exceptionType, int expectedStatus, string expectedTitle)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test message")!;
        var httpContext = TestHttpContextFactory.Create("/api/test");

        var result = _factory.Create(exception, httpContext);

        result.Should().BeOfType<ObjectResult>();
        result.StatusCode.Should().Be(expectedStatus);
        result.Value.Should().BeOfType<ProblemDetails>().Which.Should().Satisfy(problem =>
        {
            problem.Status.Should().Be(expectedStatus);
            problem.Title.Should().Be(expectedTitle);
            problem.Detail.Should().Be("Test message");
            problem.Instance.Should().Be("/api/test");
            problem.Extensions["traceId"].Should().Be(httpContext.TraceIdentifier);
        });
    }

    [Fact]
    public void Create_ShouldConvertValidationException_ToValidationProblemDetails()
    {
        var exception = new ValidationException("Name is required");
        var httpContext = TestHttpContextFactory.Create("/api/customers");

        var result = _factory.Create(exception, httpContext);

        result.Should().BeOfType<BadRequestObjectResult>();
        result.Value.Should().BeOfType<ValidationProblemDetails>().Which.Should().Satisfy(problem =>
        {
            problem.Status.Should().Be(StatusCodes.Status400BadRequest);
            problem.Errors.Should().ContainKey(string.Empty);
            problem.Instance.Should().Be("/api/customers");
            problem.Extensions["traceId"].Should().Be(httpContext.TraceIdentifier);
        });
    }

    [Fact]
    public void Create_ShouldTranslateModelState_ToValidationProblemDetails()
    {
        var httpContext = TestHttpContextFactory.Create("/api/orders");
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("OrderId", "OrderId is required");

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);

        var result = _factory.Create(actionContext);

        result.Should().BeOfType<BadRequestObjectResult>();
        result.Value.Should().BeOfType<ValidationProblemDetails>().Which.Should().Satisfy(problem =>
        {
            problem.Errors.Should().ContainKey("OrderId");
            problem.Errors["OrderId"].Should().Contain("OrderId is required");
            problem.Instance.Should().Be("/api/orders");
            problem.Extensions["traceId"].Should().Be(httpContext.TraceIdentifier);
        });
    }
}
