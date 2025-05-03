using System;
using MediatR;

namespace AstroFinance.Domain.Common
{
    public abstract class BaseEvent : INotification
    {
        public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    }
}