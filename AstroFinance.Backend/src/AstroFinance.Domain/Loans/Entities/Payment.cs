using AstroFinance.Domain.Common;
using System;

namespace AstroFinance.Domain.Loans.Entities
{
    public class Payment : BaseAuditableEntity
    {
        public Guid LoanId { get; set; }
        public Loan Loan { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}