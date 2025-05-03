using AstroFinance.Domain.Common;
using AstroFinance.Domain.Transactions.Entities;

namespace AstroFinance.Domain.Transactions.Events
{
    public class TransactionCreatedEvent : BaseEvent
    {
        public Transaction Transaction { get; }

        public TransactionCreatedEvent(Transaction transaction)
        {
            Transaction = transaction;
        }
    }
}