using AstroFinance.Domain.Common;
using AstroFinance.Domain.Loans.Entities;
using AstroFinance.Domain.Loans.Enums;

namespace AstroFinance.Domain.Loans.Events
{
    public class LoanStatusChangedEvent : BaseEvent
    {
        public Loan Loan { get; }
        public LoanStatus OldStatus { get; }
        public LoanStatus NewStatus { get; }

        public LoanStatusChangedEvent(Loan loan, LoanStatus oldStatus, LoanStatus newStatus)
        {
            Loan = loan;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}