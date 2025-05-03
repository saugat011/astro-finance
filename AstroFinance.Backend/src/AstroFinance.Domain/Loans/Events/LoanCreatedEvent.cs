using AstroFinance.Domain.Common;
using AstroFinance.Domain.Loans.Entities;

namespace AstroFinance.Domain.Loans.Events
{
    public class LoanCreatedEvent : BaseEvent
    {
        public Loan Loan { get; }

        public LoanCreatedEvent(Loan loan)
        {
            Loan = loan;
        }
    }
}