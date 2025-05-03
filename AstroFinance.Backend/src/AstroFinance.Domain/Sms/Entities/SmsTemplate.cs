using System.Collections.Generic;
using AstroFinance.Domain.Common;

namespace AstroFinance.Domain.Sms.Entities
{
    public class SmsTemplate : BaseAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        
        // Navigation properties
        public ICollection<SmsHistory> SmsHistories { get; set; } = new List<SmsHistory>();
    }
}