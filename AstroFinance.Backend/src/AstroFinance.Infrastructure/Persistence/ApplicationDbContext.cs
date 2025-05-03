using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Audit.Entities;
using AstroFinance.Domain.Auth.Entities;
using AstroFinance.Domain.Common;
using AstroFinance.Domain.Customers.Entities;
using AstroFinance.Domain.Loans.Entities;
using AstroFinance.Domain.Sms.Entities;
using AstroFinance.Domain.Transactions.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService,
            IDateTime dateTime) : base(options)
        {
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        // Auth
        public DbSet<User> Users => Set<User>();

        // Customers
        public DbSet<Customer> Customers => Set<Customer>();

        // Loans
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<PaymentSchedule> PaymentSchedules => Set<PaymentSchedule>();

        // Transactions
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
        public DbSet<JournalEntryDetail> JournalEntryDetails => Set<JournalEntryDetail>();
        public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();

        // SMS
        public DbSet<SmsTemplate> SmsTemplates => Set<SmsTemplate>();
        public DbSet<SmsHistory> SmsHistories => Set<SmsHistory>();

        // Audit
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = Guid.Parse(_currentUserService.UserId ?? Guid.Empty.ToString());
                        entry.Entity.CreatedAt = _dateTime.Now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = Guid.Parse(_currentUserService.UserId ?? Guid.Empty.ToString());
                        entry.Entity.LastModifiedAt = _dateTime.Now;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }
    }
}