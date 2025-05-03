namespace AstroFinance.Domain.Common;

/// <summary>
/// Base entity for all domain entities
/// </summary>
public abstract class BaseEntity : IEntity
{
    public Guid Id { get; set; }
}
