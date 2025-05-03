using AstroFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Reports.Queries.GetOverdueLoansReport
{
    public class GetOverdueLoansReportQuery : IRequest<OverdueLoansReportVm>
    {
        public int? DaysOverdue { get; set; }
    }

    public class GetOverdueLoansReportQueryHandler : IRequestHandler<GetOverdueLoansReportQuery, OverdueLoansReportVm>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;

        public GetOverdueLoansReportQueryHandler(IApplicationDbContext context, IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<OverdueLoansReportVm> Handle(GetOverdueLoansReportQuery request, CancellationToken cancellationToken)
        {
            var today = _dateTime.Now.Date;
            var daysOverdue = request.DaysOverdue ?? 1;

            var overdueLoans = await _context.Loans
                .AsNoTracking()
                .Include(l => l.Customer)
                .Include(l => l.PaymentSchedules)
                .Where(l => l.Status == Domain.Loans.Enums.LoanStatus.Active)
                .ToListAsync(cancellationToken);

            // Filter loans with overdue payments
            var loansWithOverduePayments = overdueLoans
                .Where(l => l.PaymentSchedules.Any(p => 
                    p.DueDate.Date < today.AddDays(-daysOverdue) && 
                    !p.IsPaid))
                .Select(l => new OverdueLoanDto
                {
                    Id = l.Id,
                    CustomerId = Guid.Parse(l.CustomerId),
                    CustomerName = $"{l.Customer.FirstName} {l.Customer.LastName}",
                    PhoneNumber = l.Customer.PhoneNumber,
                    LoanAmount = l.Amount,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    OverdueAmount = l.PaymentSchedules
                        .Where(p => p.DueDate.Date < today && !p.IsPaid)
                        .Sum(p => p.Amount),
                    DaysOverdue = (int)(today - l.PaymentSchedules
                        .Where(p => p.DueDate.Date < today && !p.IsPaid)
                        .Min(p => p.DueDate)).TotalDays,
                    NextPaymentDue = l.PaymentSchedules
                        .Where(p => !p.IsPaid)
                        .OrderBy(p => p.DueDate)
                        .FirstOrDefault()?.DueDate ?? DateTime.MinValue
                })
                .OrderByDescending(l => l.DaysOverdue)
                .ToList();

            return new OverdueLoansReportVm
            {
                OverdueLoans = loansWithOverduePayments,
                TotalCount = loansWithOverduePayments.Count,
                TotalOverdueAmount = loansWithOverduePayments.Sum(l => l.OverdueAmount),
                GeneratedDate = _dateTime.Now,
                DaysOverdueFilter = daysOverdue
            };
        }
    }

    public class OverdueLoansReportVm
    {
        public List<OverdueLoanDto> OverdueLoans { get; set; } = new List<OverdueLoanDto>();
        public int TotalCount { get; set; }
        public decimal TotalOverdueAmount { get; set; }
        public DateTime GeneratedDate { get; set; }
        public int DaysOverdueFilter { get; set; }
    }

    public class OverdueLoanDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public int DaysOverdue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime NextPaymentDue { get; set; }
    }
}