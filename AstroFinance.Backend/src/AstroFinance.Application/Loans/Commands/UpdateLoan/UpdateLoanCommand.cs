using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Loans.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Loans.Commands.UpdateLoan
{
    public class UpdateLoanCommand : IRequest
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateLoanCommandHandler : IRequestHandler<UpdateLoanCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateLoanCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task Handle(UpdateLoanCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Loans.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Loan), request.Id);
            }

            // Only allow updates if the loan is in Pending status
            if (entity.Status != Domain.Loans.Enums.LoanStatus.Pending)
            {
                throw new InvalidOperationException("Only pending loans can be updated");
            }

            entity.Amount = request.Amount;
            entity.InterestRate = request.InterestRate;
            entity.Term = request.Term;
            // No description field in Loan entity
            entity.LastModifiedBy = _currentUserService.UserId ?? Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}