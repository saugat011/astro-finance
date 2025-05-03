using System;
using AstroFinance.Domain.Common;

namespace AstroFinance.Domain.Audit.Entities
{
    public class AuditLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Create, Update, Delete
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? AffectedColumns { get; set; }
    }
}