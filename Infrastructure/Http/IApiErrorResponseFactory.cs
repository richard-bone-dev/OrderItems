namespace Api.Infrastructure.Http;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public interface IApiErrorResponseFactory
{
    ObjectResult Create(Exception exception, HttpContext httpContext);

    ObjectResult Create(ActionContext actionContext);
}
