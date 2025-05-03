using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Application.Transactions.Queries.GetTransactionsList;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Transactions.Queries.GetTransactionById
{
    public class GetTransactionByIdQuery : IRequest<TransactionDto>
    {
        public Guid Id { get; set; }
    }

    public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
    {
        private readonly IApplicationDbContext _context;

        public GetTransactionByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Loan)
                .ThenInclude(l => l != null ? l.Customer : null)
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (transaction == null)
            {
                throw new NotFoundException("Transaction", request.Id);
            }

            return new TransactionDto
            {
                Id = transaction.Id,
                LoanId = transaction.LoanId != null ? Guid.Parse(transaction.LoanId) : Guid.Empty,
                CustomerName = transaction.Loan?.Customer != null 
                    ? $"{transaction.Loan.Customer.FirstName} {transaction.Loan.Customer.LastName}" 
                    : (transaction.Customer != null ? $"{transaction.Customer.FirstName} {transaction.Customer.LastName}" : "Unknown"),
                Type = transaction.Type.ToString(),
                Amount = transaction.Amount,
                TransactionDate = transaction.Date,
                Description = transaction.Description
            };
        }
    }
}