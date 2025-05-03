using AstroFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Reports.Queries.GetTransactionSummaryReport
{
    public class GetTransactionSummaryReportQuery : IRequest<TransactionSummaryReportVm>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class GetTransactionSummaryReportQueryHandler : IRequestHandler<GetTransactionSummaryReportQuery, TransactionSummaryReportVm>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;

        public GetTransactionSummaryReportQueryHandler(IApplicationDbContext context, IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<TransactionSummaryReportVm> Handle(GetTransactionSummaryReportQuery request, CancellationToken cancellationToken)
        {
            // Start with a base query
            var baseQuery = _context.Transactions.AsNoTracking();

            if (request.FromDate.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.Date >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.Date <= request.ToDate.Value);
            }

            // Execute the query and load related data
            var transactions = await baseQuery.ToListAsync(cancellationToken);

            // Load related entities separately to avoid the ThenInclude issue
            var loanIds = transactions.Where(t => t.LoanId != null).Select(t => t.LoanId).Distinct().ToList();
            var customerIds = transactions.Select(t => t.CustomerId).Distinct().ToList();

            var loans = await _context.Loans
                .Where(l => loanIds.Contains(l.Id.ToString()))
                .Include(l => l.Customer)
                .ToDictionaryAsync(l => l.Id.ToString(), cancellationToken);

            var customers = await _context.Customers
                .Where(c => customerIds.Contains(c.Id.ToString()))
                .ToDictionaryAsync(c => c.Id.ToString(), cancellationToken);

            // Calculate summary statistics
            var totalTransactions = transactions.Count;
            var totalAmount = transactions.Sum(t => t.Amount);
            var averageAmount = totalTransactions > 0 ? totalAmount / totalTransactions : 0;

            // Group by transaction type
            var typeSummary = transactions
                .GroupBy(t => t.Type)
                .Select(g => new TransactionTypeSummaryDto
                {
                    Type = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToList();

            // Group by month
            var monthlySummary = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new MonthlyTransactionSummaryDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();

            // Recent transactions
            var recentTransactions = transactions
                .OrderByDescending(t => t.Date)
                .Take(10)
                .Select(t => 
                {
                    string customerName = "Unknown";
                    if (t.LoanId != null && loans.TryGetValue(t.LoanId, out var loan) && loan.Customer != null)
                    {
                        customerName = $"{loan.Customer.FirstName} {loan.Customer.LastName}";
                    }
                    else if (customers.TryGetValue(t.CustomerId, out var customer))
                    {
                        customerName = $"{customer.FirstName} {customer.LastName}";
                    }

                    return new TransactionDto
                    {
                        Id = t.Id,
                        LoanId = t.LoanId != null ? Guid.Parse(t.LoanId) : Guid.Empty,
                        CustomerName = customerName,
                        Type = t.Type.ToString(),
                        Amount = t.Amount,
                        TransactionDate = t.Date,
                        Description = t.Description
                    };
                })
                .ToList();

            return new TransactionSummaryReportVm
            {
                TotalTransactions = totalTransactions,
                TotalAmount = totalAmount,
                AverageAmount = averageAmount,
                TypeSummary = typeSummary,
                MonthlySummary = monthlySummary,
                RecentTransactions = recentTransactions,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                GeneratedDate = _dateTime.Now
            };
        }
    }

    public class TransactionSummaryReportVm
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public List<TransactionTypeSummaryDto> TypeSummary { get; set; } = new List<TransactionTypeSummaryDto>();
        public List<MonthlyTransactionSummaryDto> MonthlySummary { get; set; } = new List<MonthlyTransactionSummaryDto>();
        public List<TransactionDto> RecentTransactions { get; set; } = new List<TransactionDto>();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    public class TransactionTypeSummaryDto
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MonthlyTransactionSummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid LoanId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}