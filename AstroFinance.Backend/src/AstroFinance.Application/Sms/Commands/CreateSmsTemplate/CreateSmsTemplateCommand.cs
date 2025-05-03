using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Application.Sms.Queries.GetSmsTemplatesList;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Sms.Commands.CreateSmsTemplate
{
    public class CreateSmsTemplateCommand : IRequest<SmsTemplateDto>
    {
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class CreateSmsTemplateCommandHandler : IRequestHandler<CreateSmsTemplateCommand, SmsTemplateDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public CreateSmsTemplateCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _context = context;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<SmsTemplateDto> Handle(CreateSmsTemplateCommand request, CancellationToken cancellationToken)
        {
            var entity = new Domain.Sms.Entities.SmsTemplate
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Content = request.Content,
                Created = _dateTime.Now,
                CreatedBy = _currentUserService.UserId
            };

            _context.SmsTemplates.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return new SmsTemplateDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Content = entity.Content,
                Created = entity.Created,
                CreatedBy = entity.CreatedBy,
                LastModified = entity.LastModified,
                LastModifiedBy = entity.LastModifiedBy
            };
        }
    }
}