namespace Api.Application.Abstractions;

public interface ICommandHandlerAsync<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}