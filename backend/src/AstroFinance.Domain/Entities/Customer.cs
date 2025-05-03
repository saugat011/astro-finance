using System;
using System.Collections.Generic;
using AstroFinance.Domain.Common;

namespace AstroFinance.Domain.Entities
{
    public class Customer : BaseAuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty; // National ID, Passport, etc.
        public string IdentificationType { get; set; } = string.Empty;
        
        // Navigation properties
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        
        // Helper properties
        public string FullName => $"{FirstName} {LastName}";
    }
}