using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Customers.Entities;
using AstroFinance.Domain.Loans.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Loans.Queries.GetLoansList
{
    public class GetLoansListQuery : IRequest<LoansListVm>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? CustomerId { get; set; }
        public string? Status { get; set; }
    }

    public class GetLoansListQueryHandler : IRequestHandler<GetLoansListQuery, LoansListVm>
    {
        private readonly IApplicationDbContext _context;

        public GetLoansListQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoansListVm> Handle(GetLoansListQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Loan> query = _context.Loans.AsNoTracking()
                .Include(l => l.Customer);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(l => 
                    l.Customer.FirstName.ToLower().Contains(searchTerm) ||
                    l.Customer.LastName.ToLower().Contains(searchTerm)
                );
            }

            if (request.CustomerId.HasValue)
            {
                query = query.Where(l => l.CustomerId.ToString() == request.CustomerId.ToString());
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(l => l.Status.ToString() == request.Status);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var loans = await query
                .OrderByDescending(l => l.Created)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(l => new LoanDto
                {
                    Id = l.Id,
                    CustomerId = Guid.Parse(l.CustomerId),
                    CustomerName = $"{l.Customer.FirstName} {l.Customer.LastName}",
                    Amount = l.Amount,
                    InterestRate = l.InterestRate,
                    Term = l.Term,
                    Status = l.Status.ToString(),
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Created = l.Created
                })
                .ToListAsync(cancellationToken);

            return new LoansListVm
            {
                Loans = loans,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }
    }

    public class LoansListVm
    {
        public List<LoanDto> Loans { get; set; } = new List<LoanDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class LoanDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Created { get; set; }
    }
}