using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Loans.Entities;
using AstroFinance.Domain.Transactions.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Loans.Queries.GetPaymentSchedule
{
    public class GetPaymentScheduleQuery : IRequest<PaymentScheduleVm>
    {
        public Guid LoanId { get; set; }
    }

    public class GetPaymentScheduleQueryHandler : IRequestHandler<GetPaymentScheduleQuery, PaymentScheduleVm>
    {
        private readonly IApplicationDbContext _context;

        public GetPaymentScheduleQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentScheduleVm> Handle(GetPaymentScheduleQuery request, CancellationToken cancellationToken)
        {
            var loan = await _context.Loans
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

            if (loan == null)
            {
                throw new NotFoundException(nameof(Loan), request.LoanId);
            }

            // Get actual payments from transactions
            var actualPayments = await _context.Transactions
                .Where(t => t.LoanId == request.LoanId.ToString() && t.Type == Domain.Transactions.Enums.TransactionType.Repayment)
                .OrderBy(t => t.Date)
                .ToListAsync(cancellationToken);

            // Generate payment schedule
            var schedule = GeneratePaymentSchedule(loan, actualPayments);

            return new PaymentScheduleVm
            {
                LoanId = loan.Id,
                TotalAmount = loan.Amount,
                Term = loan.Term,
                InterestRate = loan.InterestRate,
                PaymentSchedule = schedule
            };
        }

        private List<PaymentScheduleItemDto> GeneratePaymentSchedule(Loan loan, List<Transaction> actualPayments)
        {
            var schedule = new List<PaymentScheduleItemDto>();
            
            var startDate = loan.StartDate;
            var monthlyPayment = CalculateMonthlyPayment(loan.Amount, loan.InterestRate, loan.Term);
            var remainingPrincipal = loan.Amount;

            for (int i = 1; i <= loan.Term; i++)
            {
                var dueDate = startDate.AddMonths(i);
                var interestPayment = remainingPrincipal * (loan.InterestRate / 100 / 12);
                var principalPayment = monthlyPayment - interestPayment;
                
                if (i == loan.Term)
                {
                    // Last payment - adjust for rounding errors
                    principalPayment = remainingPrincipal;
                    monthlyPayment = principalPayment + interestPayment;
                }

                remainingPrincipal -= principalPayment;

                // Find actual payment for this period
                var actualPayment = actualPayments.FirstOrDefault(p => 
                    p.Date.Year == dueDate.Year && 
                    p.Date.Month == dueDate.Month);

                schedule.Add(new PaymentScheduleItemDto
                {
                    PaymentNumber = i,
                    DueDate = dueDate,
                    ExpectedAmount = monthlyPayment,
                    PrincipalComponent = principalPayment,
                    InterestComponent = interestPayment,
                    RemainingPrincipal = remainingPrincipal > 0 ? remainingPrincipal : 0,
                    ActualPaymentDate = actualPayment?.Date,
                    ActualPaymentAmount = actualPayment?.Amount ?? 0,
                    Status = GetPaymentStatus(dueDate, actualPayment)
                });
            }

            return schedule;
        }

        private decimal CalculateMonthlyPayment(decimal principal, decimal annualInterestRate, int termInMonths)
        {
            var monthlyRate = annualInterestRate / 100 / 12;
            
            if (monthlyRate == 0)
            {
                return principal / termInMonths;
            }

            var factor = (decimal)Math.Pow(1 + (double)monthlyRate, termInMonths);
            return principal * monthlyRate * factor / (factor - 1);
        }

        private string GetPaymentStatus(DateTime dueDate, Transaction? actualPayment)
        {
            if (actualPayment == null)
            {
                return dueDate > DateTime.Now ? "Upcoming" : "Overdue";
            }
            
            return actualPayment.Date <= dueDate ? "Paid" : "Late";
        }
    }

    public class PaymentScheduleVm
    {
        public Guid LoanId { get; set; }
        public decimal TotalAmount { get; set; }
        public int Term { get; set; }
        public decimal InterestRate { get; set; }
        public List<PaymentScheduleItemDto> PaymentSchedule { get; set; } = new List<PaymentScheduleItemDto>();
    }

    public class PaymentScheduleItemDto
    {
        public int PaymentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal PrincipalComponent { get; set; }
        public decimal InterestComponent { get; set; }
        public decimal RemainingPrincipal { get; set; }
        public DateTime? ActualPaymentDate { get; set; }
        public decimal ActualPaymentAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}