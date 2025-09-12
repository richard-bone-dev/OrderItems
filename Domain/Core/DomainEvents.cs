namespace Api.Domain.Core;

public abstract class DomainEvent { }

public static class DomainEvents
{
    public static event Action<DomainEvent> Handlers;
    public static void Register(Action<DomainEvent> handler) => Handlers += handler;
    public static void Raise(DomainEvent domainEvent) => Handlers?.Invoke(domainEvent);
}