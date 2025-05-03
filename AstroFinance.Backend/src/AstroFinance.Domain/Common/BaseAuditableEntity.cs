using System.ComponentModel.DataAnnotations.Schema;

namespace AstroFinance.Domain.Common;

/// <summary>
/// Base auditable entity that includes audit fields
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    [Column("created_at")]  // Explicit column mapping
    public DateTime CreatedAt { get; set; }
    
    [Column("created_by")]
    public Guid? CreatedBy { get; set; }
    
    [Column("last_modified_at")]
    public DateTime? LastModifiedAt { get; set; }
    
    [Column("last_modified_by")]
    public Guid? LastModifiedBy { get; set; }
}