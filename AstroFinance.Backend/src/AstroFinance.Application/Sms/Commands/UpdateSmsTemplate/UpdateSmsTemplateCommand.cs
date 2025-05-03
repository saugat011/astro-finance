using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Sms.Commands.UpdateSmsTemplate
{
    public class UpdateSmsTemplateCommand : IRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateSmsTemplateCommandHandler : IRequestHandler<UpdateSmsTemplateCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public UpdateSmsTemplateCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _context = context;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task Handle(UpdateSmsTemplateCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.SmsTemplates
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException("SmsTemplate", request.Id);
            }

            entity.Name = request.Name;
            entity.Content = request.Content;
            entity.LastModified = _dateTime.Now;
            entity.LastModifiedBy = _currentUserService.UserId;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}