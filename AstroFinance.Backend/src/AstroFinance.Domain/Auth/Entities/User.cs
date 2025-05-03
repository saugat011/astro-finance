using System;
using System.Collections.Generic;
using AstroFinance.Domain.Common;
using AstroFinance.Domain.Loans.Entities;
using AstroFinance.Domain.Transactions.Entities;

namespace AstroFinance.Domain.Auth.Entities
{
    public class User : BaseAuditableEntity
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }

        // Navigation properties
        public ICollection<Loan> CreatedLoans { get; set; } = new List<Loan>();
        public ICollection<Transaction> CreatedTransactions { get; set; } = new List<Transaction>();
        
        // Helper properties
        public string FullName => $"{FirstName} {LastName}";
    }
}