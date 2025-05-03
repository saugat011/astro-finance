using System;
using System.Collections.Generic;
using System.Linq;
using AstroFinance.Domain.Common;
using AstroFinance.Domain.Customers.Entities;
using AstroFinance.Domain.Loans.Enums;
using AstroFinance.Domain.Transactions.Entities;
using AstroFinance.Domain.Transactions.Enums;

namespace AstroFinance.Domain.Loans.Entities
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
        public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
        
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
        
        public void GeneratePaymentSchedule()
        {
            PaymentSchedules.Clear();
            
            decimal emi = CalculateEMI();
            DateTime dueDate = StartDate;
            
            if (Type == LoanType.Flat)
            {
                decimal monthlyInterest = Amount * (InterestRate / 100) / 12;
                decimal monthlyPrincipal = Amount / Term;
                
                for (int i = 1; i <= Term; i++)
                {
                    dueDate = dueDate.AddMonths(1);
                    
                    var payment = new PaymentSchedule
                    {
                        LoanId = Id,
                        DueDate = dueDate,
                        Amount = emi,
                        Principal = monthlyPrincipal,
                        Interest = monthlyInterest,
                        IsPaid = false
                    };
                    
                    PaymentSchedules.Add(payment);
                }
            }
            else // Diminishing
            {
                decimal balance = Amount;
                decimal monthlyRate = InterestRate / 100 / 12;
                
                for (int i = 1; i <= Term; i++)
                {
                    dueDate = dueDate.AddMonths(1);
                    
                    decimal interest = balance * monthlyRate;
                    decimal principal = emi - interest;
                    
                    var payment = new PaymentSchedule
                    {
                        LoanId = Id,
                        DueDate = dueDate,
                        Amount = emi,
                        Principal = principal,
                        Interest = interest,
                        IsPaid = false
                    };
                    
                    PaymentSchedules.Add(payment);
                    
                    balance -= principal;
                }
            }
        }
    }
}