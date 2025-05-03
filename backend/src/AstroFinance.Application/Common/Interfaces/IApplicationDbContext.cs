using AstroFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstroFinance.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Customer> Customers { get; }
        DbSet<Loan> Loans { get; }
        DbSet<Transaction> Transactions { get; }
        DbSet<SmsTemplate> SmsTemplates { get; }
        DbSet<SmsHistory> SmsHistories { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}