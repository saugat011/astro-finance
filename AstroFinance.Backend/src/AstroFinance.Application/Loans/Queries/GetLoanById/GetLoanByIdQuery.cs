using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Application.Loans.Queries.GetLoansList;
using AstroFinance.Domain.Loans.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Loans.Queries.GetLoanById
{
    public class GetLoanByIdQuery : IRequest<LoanDetailDto>
    {
        public Guid Id { get; set; }
    }

    public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanDetailDto>
    {
        private readonly IApplicationDbContext _context;

        public GetLoanByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoanDetailDto> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.Loans
                .Include(l => l.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Loan), request.Id);
            }

            // Get payment count and total paid amount from transactions
            var transactions = await _context.Transactions
                .Where(t => t.LoanId == request.Id.ToString() && t.Type == Domain.Transactions.Enums.TransactionType.Repayment)
                .ToListAsync(cancellationToken);
                
            var paymentCount = transactions.Count;
            var totalPaidAmount = transactions.Sum(t => t.Amount);

            return new LoanDetailDto
            {
                Id = entity.Id,
                CustomerId = Guid.Parse(entity.CustomerId),
                CustomerName = $"{entity.Customer.FirstName} {entity.Customer.LastName}",
                Amount = entity.Amount,
                InterestRate = entity.InterestRate,
                Term = entity.Term,
                Status = entity.Status.ToString(),
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Created = entity.CreatedAt,
                PaymentCount = paymentCount,
                TotalPaidAmount = totalPaidAmount,
                RemainingAmount = entity.Amount - totalPaidAmount
            };
        }
    }

    public class LoanDetailDto : LoanDto
    {
        public int PaymentCount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}