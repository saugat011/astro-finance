using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Domain.Customers.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommand : IRequest<CustomerDto>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public string IdentificationType { get; set; } = string.Empty;
    }

    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateCustomerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var entity = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                IdentificationNumber = request.IdentificationNumber,
                IdentificationType = request.IdentificationType,
                CreatedBy = _currentUserService.UserId ?? Guid.Empty
            };

            _context.Customers.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return new CustomerDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                IdentificationNumber = entity.IdentificationNumber,
                IdentificationType = entity.IdentificationType,
                CreatedAt = entity.CreatedAt
            };
        }
    }

    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public string IdentificationType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}