using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Sms.Commands.DeleteSmsTemplate
{
    public class DeleteSmsTemplateCommand : IRequest
    {
        public Guid Id { get; set; }
    }

    public class DeleteSmsTemplateCommandHandler : IRequestHandler<DeleteSmsTemplateCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteSmsTemplateCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteSmsTemplateCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.SmsTemplates
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException("SmsTemplate", request.Id);
            }

            _context.SmsTemplates.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}