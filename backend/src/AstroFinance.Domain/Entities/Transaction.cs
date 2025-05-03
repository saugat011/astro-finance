using System;
using AstroFinance.Domain.Common;
using AstroFinance.Domain.Enums;

namespace AstroFinance.Domain.Entities
{
    public class Transaction : BaseAuditableEntity
    {
        public string? LoanId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        
        // Navigation properties
        public Loan? Loan { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}