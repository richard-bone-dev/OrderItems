# Testing Utilities

This folder contains reusable helpers for API controller unit tests. Use `TestHttpContextFactory` when you need a configured `HttpContext` instance without spinning up the full ASP.NET Core pipeline. The factory ensures a predictable request path and trace identifier so assertions against `ProblemDetails` responses remain stable across test suites.
