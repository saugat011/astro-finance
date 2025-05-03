using AstroFinance.Domain.Common;
using AstroFinance.Domain.Customers.Entities;

namespace AstroFinance.Domain.Customers.Events
{
    public class CustomerCreatedEvent : BaseEvent
    {
        public Customer Customer { get; }

        public CustomerCreatedEvent(Customer customer)
        {
            Customer = customer;
        }
    }
}