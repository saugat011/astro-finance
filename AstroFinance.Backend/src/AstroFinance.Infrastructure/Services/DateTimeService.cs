using AstroFinance.Application.Common.Interfaces;
using System;

namespace AstroFinance.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.UtcNow;
    }
}