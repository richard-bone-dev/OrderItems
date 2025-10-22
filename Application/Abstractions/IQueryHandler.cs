namespace Api.Application.Abstractions;

public interface IQueryHandlerAsync<TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}