using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Loans.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Loans.Commands.ApproveLoan
{
    public class ApproveLoanCommand : IRequest
    {
        public Guid Id { get; set; }
    }

    public class ApproveLoanCommandHandler : IRequestHandler<ApproveLoanCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ApproveLoanCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task Handle(ApproveLoanCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Loans.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Loan), request.Id);
            }

            // Only allow approval if the loan is in Pending status
            if (entity.Status != Domain.Loans.Enums.LoanStatus.Pending)
            {
                throw new InvalidOperationException("Only pending loans can be approved");
            }

            entity.Status = Domain.Loans.Enums.LoanStatus.Active;
            entity.LastModifiedBy = _currentUserService.UserId ?? Guid.Empty.ToString();

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}