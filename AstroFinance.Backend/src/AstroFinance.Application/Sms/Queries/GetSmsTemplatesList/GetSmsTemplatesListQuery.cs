using AstroFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Sms.Queries.GetSmsTemplatesList
{
    public class GetSmsTemplatesListQuery : IRequest<SmsTemplatesListVm>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetSmsTemplatesListQueryHandler : IRequestHandler<GetSmsTemplatesListQuery, SmsTemplatesListVm>
    {
        private readonly IApplicationDbContext _context;

        public GetSmsTemplatesListQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SmsTemplatesListVm> Handle(GetSmsTemplatesListQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SmsTemplates.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.Name.ToLower().Contains(searchTerm) ||
                    t.Content.ToLower().Contains(searchTerm)
                );
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var templates = await query
                .OrderBy(t => t.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new SmsTemplateDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Content = t.Content,
                    Created = t.Created,
                    CreatedBy = t.CreatedBy,
                    LastModified = t.LastModified,
                    LastModifiedBy = t.LastModifiedBy
                })
                .ToListAsync(cancellationToken);

            return new SmsTemplatesListVm
            {
                Templates = templates,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }
    }

    public class SmsTemplatesListVm
    {
        public List<SmsTemplateDto> Templates { get; set; } = new List<SmsTemplateDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class SmsTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}