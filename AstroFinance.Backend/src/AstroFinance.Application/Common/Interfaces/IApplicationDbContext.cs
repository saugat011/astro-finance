using AstroFinance.Domain.Audit.Entities;
using AstroFinance.Domain.Auth.Entities;
using AstroFinance.Domain.Customers.Entities;
using AstroFinance.Domain.Loans.Entities;
using AstroFinance.Domain.Sms.Entities;
using AstroFinance.Domain.Transactions.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        // Auth
        DbSet<User> Users { get; }

        // Customers
        DbSet<Customer> Customers { get; }

        // Loans
        DbSet<Loan> Loans { get; }
        DbSet<PaymentSchedule> PaymentSchedules { get; }

        // Transactions
        DbSet<Transaction> Transactions { get; }
        DbSet<JournalEntry> JournalEntries { get; }
        DbSet<JournalEntryDetail> JournalEntryDetails { get; }
        DbSet<ChartOfAccount> ChartOfAccounts { get; }

        // SMS
        DbSet<SmsTemplate> SmsTemplates { get; }
        DbSet<SmsHistory> SmsHistories { get; }

        // Audit
        DbSet<AuditLog> AuditLogs { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}