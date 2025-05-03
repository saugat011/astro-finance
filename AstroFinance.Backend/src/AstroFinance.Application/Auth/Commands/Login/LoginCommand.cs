using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Application.Common.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Auth.Commands.Login
{
    public class LoginCommand : IRequest<AuthenticationResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResult>
    {
        private readonly IIdentityService _identityService;

        public LoginCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<AuthenticationResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.AuthenticateAsync(request.Email, request.Password);
        }
    }
}