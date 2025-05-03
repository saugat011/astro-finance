using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Loans.Entities;
using AstroFinance.Domain.Transactions.Entities;
using AstroFinance.Domain.Transactions.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Loans.Commands.DisburseLoan
{
    public class DisburseCommand : IRequest
    {
        public Guid LoanId { get; set; }
        public string DisbursementMethod { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }

    public class DisburseCommandHandler : IRequestHandler<DisburseCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public DisburseCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _context = context;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task Handle(DisburseCommand request, CancellationToken cancellationToken)
        {
            var loan = await _context.Loans.FindAsync(new object[] { request.LoanId }, cancellationToken);

            if (loan == null)
            {
                throw new NotFoundException(nameof(Loan), request.LoanId);
            }

            // Only allow disbursement if the loan is in Pending status
            if (loan.Status != Domain.Loans.Enums.LoanStatus.Pending)
            {
                throw new InvalidOperationException("Only pending loans can be disbursed");
            }

            // Update loan status and dates
            var now = _dateTime.Now;
            loan.Status = Domain.Loans.Enums.LoanStatus.Active;
            loan.StartDate = now;
            loan.EndDate = now.AddMonths(loan.Term);
            loan.LastModifiedBy = _currentUserService.UserId ?? Guid.Empty.ToString();

            // Create disbursement transaction
            var transaction = new Transaction
            {
                LoanId = loan.Id.ToString(),
                CustomerId = loan.CustomerId,
                Type = TransactionType.Disbursement,
                Amount = loan.Amount,
                Date = now,
                Description = $"Loan disbursement for loan ID {loan.Id}",
                CreatedBy = _currentUserService.UserId ?? Guid.Empty.ToString()
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}