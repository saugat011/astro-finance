using System;
using AstroFinance.Domain.Common;
using AstroFinance.Domain.Enums;

namespace AstroFinance.Domain.Entities
{
    public class SmsHistory : BaseAuditableEntity
    {
        public string RecipientNumber { get; set; } = string.Empty;
        public string? RecipientName { get; set; }
        public string Message { get; set; } = string.Empty;
        public SmsStatus Status { get; set; } = SmsStatus.Pending;
        public DateTime SentAt { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SmsTemplateId { get; set; }
        
        // Navigation properties
        public SmsTemplate? SmsTemplate { get; set; }
    }
}