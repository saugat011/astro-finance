using AstroFinance.Application.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<RegisterResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
    {
        private readonly IIdentityService _identityService;
        private readonly IApplicationDbContext _context;

        public RegisterCommandHandler(IIdentityService identityService, IApplicationDbContext context)
        {
            _identityService = identityService;
            _context = context;
        }

        public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var (result, userId) = await _identityService.CreateUserAsync(request.Email, request.Password);

            if (!result.Succeeded)
            {
                return new RegisterResult
                {
                    Success = false,
                    Errors = result.Errors
                };
            }

            // Update user details
            var user = await _context.Users.FindAsync(new object[] { System.Guid.Parse(userId) }, cancellationToken);
            if (user != null)
            {
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Role = request.Role;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new RegisterResult
            {
                Success = true,
                UserId = userId
            };
        }
    }

    public class RegisterResult
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string[]? Errors { get; set; }
    }
}