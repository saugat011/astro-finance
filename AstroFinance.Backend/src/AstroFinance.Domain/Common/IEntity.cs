namespace AstroFinance.Domain.Common;

/// <summary>
/// Interface for all entities
/// </summary>
public interface IEntity
{
    Guid Id { get; set; }
}