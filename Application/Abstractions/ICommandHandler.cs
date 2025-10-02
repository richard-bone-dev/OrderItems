namespace Api.Application.Abstractions;

public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken ct = default);
}