using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Application.Loans.Queries.GetLoansList;
using AstroFinance.Domain.Loans.Entities;
using AstroFinance.Domain.Loans.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Loans.Commands.CreateLoan
{
    public class CreateLoanCommand : IRequest<LoanDto>
    {
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; } // in months
        public string? Description { get; set; }
    }

    public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, LoanDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public CreateLoanCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _context = context;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<LoanDto> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
        {
            var loanNumber = GenerateLoanNumber();

            var entity = new Loan
            {
                CustomerId = request.CustomerId.ToString(),
                Amount = request.Amount,
                InterestRate = request.InterestRate,
                Term = request.Term,
                Status = LoanStatus.Pending,
                Type = LoanType.Flat, // Default to Flat rate
                StartDate = _dateTime.Now,
                EndDate = _dateTime.Now.AddMonths(request.Term),
                CreatedBy = _currentUserService.UserId ?? Guid.Empty.ToString()
            };

            _context.Loans.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Get customer name for the response
            var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
            var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : string.Empty;

            return new LoanDto
            {
                Id = entity.Id,
                CustomerId = Guid.Parse(entity.CustomerId),
                CustomerName = customerName,
                Amount = entity.Amount,
                InterestRate = entity.InterestRate,
                Term = entity.Term,
                Status = entity.Status.ToString(),
                Created = entity.Created
            };
        }

        private string GenerateLoanNumber()
        {
            // Generate a unique loan number with prefix LN followed by year and a random number
            var year = _dateTime.Now.Year.ToString().Substring(2);
            var random = new Random();
            var randomNumber = random.Next(10000, 99999);
            return $"LN{year}{randomNumber}";
        }
    }
}