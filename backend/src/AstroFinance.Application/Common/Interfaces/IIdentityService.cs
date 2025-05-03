using AstroFinance.Application.Common.Models;

namespace AstroFinance.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<string?> GetUserNameAsync(string userId);
        
        Task<bool> IsInRoleAsync(string userId, string role);
        
        Task<bool> AuthorizeAsync(string userId, string policyName);
        
        Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);
        
        Task<Result> DeleteUserAsync(string userId);
        
        Task<Result> UpdateUserAsync(string userId, string userName, string? password = null);
        
        Task<(Result Result, string? Token)> AuthenticateAsync(string email, string password);
    }
}