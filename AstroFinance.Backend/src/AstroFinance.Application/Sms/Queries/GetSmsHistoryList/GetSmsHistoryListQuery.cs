using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Sms.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Sms.Queries.GetSmsHistoryList
{
    public class GetSmsHistoryListQuery : IRequest<SmsHistoryListVm>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? CustomerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class GetSmsHistoryListQueryHandler : IRequestHandler<GetSmsHistoryListQuery, SmsHistoryListVm>
    {
        private readonly IApplicationDbContext _context;

        public GetSmsHistoryListQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SmsHistoryListVm> Handle(GetSmsHistoryListQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SmsHistories.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(s => 
                    s.Message.ToLower().Contains(searchTerm) ||
                    s.RecipientName != null && s.RecipientName.ToLower().Contains(searchTerm) ||
                    s.RecipientNumber.Contains(searchTerm)
                );
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(s => s.SentAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(s => s.SentAt <= request.ToDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var smsHistory = await query
                .OrderByDescending(s => s.SentAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new SmsHistoryDto
                {
                    Id = s.Id,
                    RecipientName = s.RecipientName ?? string.Empty,
                    PhoneNumber = s.RecipientNumber,
                    Message = s.Message,
                    Status = s.Status.ToString(),
                    SentDate = s.SentAt,
                    ErrorMessage = s.ErrorMessage
                })
                .ToListAsync(cancellationToken);

            return new SmsHistoryListVm
            {
                SmsHistory = smsHistory,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }
    }

    public class SmsHistoryListVm
    {
        public List<SmsHistoryDto> SmsHistory { get; set; } = new List<SmsHistoryDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class SmsHistoryDto
    {
        public Guid Id { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public string? ErrorMessage { get; set; }
    }
}