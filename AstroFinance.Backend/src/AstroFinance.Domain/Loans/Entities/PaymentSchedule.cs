using System;
using AstroFinance.Domain.Common;

namespace AstroFinance.Domain.Loans.Entities
{
    public class PaymentSchedule : BaseAuditableEntity
    {
        public string LoanId { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? TransactionId { get; set; }
        
        // Navigation properties
        public Loan Loan { get; set; } = null!;
    }
}