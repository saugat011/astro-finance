using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Sms.Entities;
using AstroFinance.Domain.Sms.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Sms.Commands.SendSms
{
    public class SendSmsCommand : IRequest<SendSmsResult>
    {
        public Guid CustomerId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SendSmsCommandHandler : IRequestHandler<SendSmsCommand, SendSmsResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISmsService _smsService;
        private readonly IDateTime _dateTime;

        public SendSmsCommandHandler(
            IApplicationDbContext context,
            ISmsService smsService,
            IDateTime dateTime)
        {
            _context = context;
            _smsService = smsService;
            _dateTime = dateTime;
        }

        public async Task<SendSmsResult> Handle(SendSmsCommand request, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id.ToString() == request.CustomerId.ToString(), cancellationToken);

            if (customer == null)
            {
                throw new NotFoundException("Customer", request.CustomerId);
            }

            // Send SMS using the SMS service
            var smsResult = await _smsService.SendSmsAsync(customer.PhoneNumber, request.Message);

            // Record the SMS in history
            var smsHistory = new SmsHistory
            {
                Id = Guid.NewGuid(),
                RecipientNumber = customer.PhoneNumber,
                RecipientName = $"{customer.FirstName} {customer.LastName}",
                Message = request.Message,
                Status = smsResult.Success ? SmsStatus.Sent : SmsStatus.Failed,
                SentAt = _dateTime.Now,
                ErrorMessage = smsResult.Success ? null : smsResult.ErrorMessage
            };

            _context.SmsHistories.Add(smsHistory);
            await _context.SaveChangesAsync(cancellationToken);

            return new SendSmsResult
            {
                Success = smsResult.Success,
                MessageId = smsHistory.Id,
                Message = smsResult.Success ? "SMS sent successfully" : smsResult.ErrorMessage ?? "Failed to send SMS"
            };
        }
    }

    public class SendSmsResult
    {
        public bool Success { get; set; }
        public Guid MessageId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}