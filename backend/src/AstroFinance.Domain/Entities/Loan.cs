using System;
using System.Collections.Generic;
using AstroFinance.Domain.Common;
using AstroFinance.Domain.Enums;

namespace AstroFinance.Domain.Entities
{
    public class Loan : BaseAuditableEntity
    {
        public string CustomerId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; } // In months
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Pending;
        public LoanType Type { get; set; }
        
        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        
        // Calculated properties
        public decimal GetTotalInterest()
        {
            if (Type == LoanType.Flat)
            {
                return Amount * (InterestRate / 100) * (Term / 12.0m);
            }
            else // Diminishing
            {
                // This is a simplified calculation - in a real app, you'd use a more complex amortization formula
                decimal monthlyRate = InterestRate / 100 / 12;
                decimal totalPayment = CalculateEMI() * Term;
                return totalPayment - Amount;
            }
        }
        
        public decimal CalculateEMI()
        {
            if (Type == LoanType.Flat)
            {
                decimal monthlyInterest = Amount * (InterestRate / 100) / 12;
                decimal monthlyPrincipal = Amount / Term;
                return monthlyPrincipal + monthlyInterest;
            }
            else // Diminishing
            {
                decimal monthlyRate = InterestRate / 100 / 12;
                decimal emi = Amount * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), Term) 
                    / ((decimal)Math.Pow((double)(1 + monthlyRate), Term) - 1);
                return emi;
            }
        }
        
        public decimal GetOutstandingBalance()
        {
            decimal totalRepaid = Transactions
                .Where(t => t.Type == TransactionType.Repayment)
                .Sum(t => t.Amount);
                
            if (Type == LoanType.Flat)
            {
                return Amount + GetTotalInterest() - totalRepaid;
            }
            else
            {
                // For diminishing, we'd need to calculate based on amortization schedule
                // This is simplified - in a real app, you'd track principal and interest separately
                return Amount + GetTotalInterest() - totalRepaid;
            }
        }
    }
}