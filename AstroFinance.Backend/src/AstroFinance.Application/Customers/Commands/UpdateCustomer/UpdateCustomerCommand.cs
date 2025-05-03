using AstroFinance.Application.Common.Exceptions;
using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Customers.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommand : IRequest
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public string IdentificationType { get; set; } = string.Empty;
    }

    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCustomerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Customers.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Customer), request.Id);
            }

            entity.FirstName = request.FirstName;
            entity.LastName = request.LastName;
            entity.Email = request.Email;
            entity.PhoneNumber = request.PhoneNumber;
            entity.Address = request.Address;
            entity.IdentificationNumber = request.IdentificationNumber;
            entity.IdentificationType = request.IdentificationType;
            entity.LastModifiedBy = _currentUserService.UserId ?? Guid.Empty.ToString();

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}