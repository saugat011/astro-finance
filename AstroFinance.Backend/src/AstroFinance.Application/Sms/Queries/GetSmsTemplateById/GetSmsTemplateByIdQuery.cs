using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Application.Sms.Queries.GetSmsTemplatesList;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Sms.Queries.GetSmsTemplateById
{
    public class GetSmsTemplateByIdQuery : IRequest<SmsTemplateDto>
    {
        public Guid Id { get; set; }
    }

    public class GetSmsTemplateByIdQueryHandler : IRequestHandler<GetSmsTemplateByIdQuery, SmsTemplateDto>
    {
        private readonly IApplicationDbContext _context;

        public GetSmsTemplateByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SmsTemplateDto> Handle(GetSmsTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var template = await _context.SmsTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (template == null)
            {
                throw new NotFoundException("SmsTemplate", request.Id);
            }

            return new SmsTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Content = template.Content,
                CreatedAt = template.CreatedAt,
                CreatedBy = template.CreatedBy,
                LastModifiedAt = template.LastModifiedAt,
                LastModifiedBy = template.LastModifiedBy
            };
        }
    }
}