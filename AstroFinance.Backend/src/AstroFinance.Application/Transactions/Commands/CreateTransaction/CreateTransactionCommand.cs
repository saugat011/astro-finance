using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Transactions.Entities;
using AstroFinance.Domain.Transactions.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Transactions.Commands.CreateTransaction
{
    public class CreateTransactionCommand : IRequest<Guid>
    {
        public Guid LoanId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public CreateTransactionCommandHandler(
            IApplicationDbContext context,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _dateTime = dateTime;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            var loan = await _context.Loans
                .Include(l => l.Customer)
                .FirstOrDefaultAsync(l => l.Id.ToString() == request.LoanId.ToString(), cancellationToken);

            if (loan == null)
            {
                throw new NotFoundException("Loan", request.LoanId);
            }

            if (!Enum.TryParse<TransactionType>(request.Type, true, out var transactionType))
            {
                throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                {
                    new FluentValidation.Results.ValidationFailure("Type", "Invalid transaction type")
                });
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                LoanId = request.LoanId.ToString(),
                CustomerId = loan.CustomerId,
                Type = transactionType,
                Amount = request.Amount,
                Date = request.TransactionDate ?? _dateTime.Now,
                Description = request.Description,
                CreatedAt = _dateTime.Now,
                CreatedBy = _currentUserService.UserId
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return transaction.Id;
        }
    }
}