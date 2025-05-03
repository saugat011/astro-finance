using AstroFinance.Application.Common.Models;
using AstroFinance.Application.Users.Queries.GetUsersList;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AstroFinance.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<string?> GetUserNameAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<bool> AuthorizeAsync(string userId, string policyName);
        Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);
        Task<Result> DeleteUserAsync(string userId);
        Task<AuthenticationResult> AuthenticateAsync(string email, string password);
        
        // User management methods
        Task<(List<UserDto> Users, int TotalCount)> GetUsersAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<Result> UpdateUserAsync(Guid userId, string email, string firstName, string lastName, List<string> roles, bool isActive, CancellationToken cancellationToken);
        Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
    }
}