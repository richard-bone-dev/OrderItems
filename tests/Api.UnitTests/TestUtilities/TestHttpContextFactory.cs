namespace Api.Tests.TestUtilities;

using Microsoft.AspNetCore.Http;

public static class TestHttpContextFactory
{
    public static DefaultHttpContext Create(string path = "/test")
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = Guid.NewGuid().ToString()
        };

        context.Request.Path = path;

        return context;
    }
}
