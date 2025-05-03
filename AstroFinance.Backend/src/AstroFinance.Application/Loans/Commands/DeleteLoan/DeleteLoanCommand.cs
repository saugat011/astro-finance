using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Loans.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Loans.Commands.DeleteLoan
{
    public class DeleteLoanCommand : IRequest
    {
        public Guid Id { get; set; }
    }

    public class DeleteLoanCommandHandler : IRequestHandler<DeleteLoanCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteLoanCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteLoanCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Loans.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Loan), request.Id);
            }

            // Only allow deletion if the loan is in Pending status
            if (entity.Status != Domain.Loans.Enums.LoanStatus.Pending)
            {
                throw new InvalidOperationException("Only pending loans can be deleted");
            }

            // Check if loan has any transactions
            var hasTransactions = await _context.Transactions
                .Where(t => t.LoanId == request.Id.ToString())
                .AnyAsync(cancellationToken);

            if (hasTransactions)
            {
                throw new InvalidOperationException("Cannot delete loan with existing transactions");
            }

            _context.Loans.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}