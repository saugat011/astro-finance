using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Application.Customers.Commands.CreateCustomer;
using AstroFinance.Domain.Customers.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQuery : IRequest<CustomerDetailDto>
    {
        public Guid Id { get; set; }
    }

    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomerByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDetailDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Customer), request.Id);
            }

            // Get loan count
            var loanCount = await _context.Loans
                .CountAsync(l => l.CustomerId.ToString() == request.Id.ToString(), cancellationToken);

            // Get active loan count
            var activeLoanCount = await _context.Loans
                .CountAsync(l => l.CustomerId.ToString() == request.Id.ToString() && l.Status == Domain.Loans.Enums.LoanStatus.Active, cancellationToken);

            // Get total loan amount
            var totalLoanAmount = await _context.Loans
                .Where(l => l.CustomerId.ToString() == request.Id.ToString())
                .SumAsync(l => l.Amount, cancellationToken);

            return new CustomerDetailDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                IdentificationNumber = entity.IdentificationNumber,
                IdentificationType = entity.IdentificationType,
                CreatedAt = entity.CreatedAt,
                LoanCount = loanCount,
                ActiveLoanCount = activeLoanCount,
                TotalLoanAmount = totalLoanAmount
            };
        }
    }

    public class CustomerDetailDto : CustomerDto
    {
        public int LoanCount { get; set; }
        public int ActiveLoanCount { get; set; }
        public decimal TotalLoanAmount { get; set; }
    }
}