namespace Api.Domain.Core;

// --- Infrastructure: Base Entity ---
public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
}
