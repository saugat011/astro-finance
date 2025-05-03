using AstroFinance.Domain.Auth.Entities;
using AstroFinance.Domain.Common;

namespace AstroFinance.Domain.Auth.Events
{
    public class UserCreatedEvent : BaseEvent
    {
        public User User { get; }

        public UserCreatedEvent(User user)
        {
            User = user;
        }
    }
}