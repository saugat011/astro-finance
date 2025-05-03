using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Customers.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Customers.Commands.DeleteCustomer
{
    public class DeleteCustomerCommand : IRequest
    {
        public Guid Id { get; set; }
    }

    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Customers.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Customer), request.Id);
            }

            // Check if customer has any active loans
            var hasActiveLoans = await _context.Loans
                .Where(l => l.CustomerId == request.Id && (l.Status == Domain.Loans.Enums.LoanStatus.Active || l.Status == Domain.Loans.Enums.LoanStatus.Pending))
                .AnyAsync(cancellationToken);

            if (hasActiveLoans)
            {
                throw new InvalidOperationException("Cannot delete customer with active loans");
            }

            _context.Customers.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}