using AstroFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Reports.Queries.GetLoanSummaryReport
{
    public class GetLoanSummaryReportQuery : IRequest<LoanSummaryReportVm>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class GetLoanSummaryReportQueryHandler : IRequestHandler<GetLoanSummaryReportQuery, LoanSummaryReportVm>
    {
        private readonly IApplicationDbContext _context;

        public GetLoanSummaryReportQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoanSummaryReportVm> Handle(GetLoanSummaryReportQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Loans.AsNoTracking();

            if (request.FromDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt <= request.ToDate.Value);
            }

            var loans = await query.ToListAsync(cancellationToken);

            // Calculate summary statistics
            var totalLoans = loans.Count;
            var totalAmount = loans.Sum(l => l.Amount);
            var averageAmount = totalLoans > 0 ? totalLoans > 0 ? totalAmount / totalLoans : 0 : 0;
            var averageInterestRate = totalLoans > 0 ? loans.Average(l => l.InterestRate) : 0;
            var averageTerm = totalLoans > 0 ? loans.Average(l => l.Term) : 0;

            // Group by status
            var statusSummary = loans
                .GroupBy(l => l.Status)
                .Select(g => new LoanStatusSummaryDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(l => l.Amount)
                })
                .ToList();

            // Group by month
            var monthlySummary = loans
                .GroupBy(l => new { l.CreatedAt.Year, l.CreatedAt.Month })
                .Select(g => new MonthlyLoanSummaryDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    TotalAmount = g.Sum(l => l.Amount)
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();

            return new LoanSummaryReportVm
            {
                TotalLoans = totalLoans,
                TotalAmount = totalAmount,
                AverageAmount = averageAmount,
                AverageInterestRate = averageInterestRate,
                AverageTerm = averageTerm,
                StatusSummary = statusSummary,
                MonthlySummary = monthlySummary,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                GeneratedDate = DateTime.UtcNow
            };
        }
    }

    public class LoanSummaryReportVm
    {
        public int TotalLoans { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal AverageInterestRate { get; set; }
        public double AverageTerm { get; set; }
        public List<LoanStatusSummaryDto> StatusSummary { get; set; } = new List<LoanStatusSummaryDto>();
        public List<MonthlyLoanSummaryDto> MonthlySummary { get; set; } = new List<MonthlyLoanSummaryDto>();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    public class LoanStatusSummaryDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MonthlyLoanSummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
}