using System;

namespace AstroFinance.Application.Common.Models
{
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public string? ErrorMessage { get; set; }

        public static AuthenticationResult SuccessResult(string token, string refreshToken, DateTime expiresAt, string userId, string userName, string userRole)
        {
            return new AuthenticationResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                UserId = userId,
                UserName = userName,
                UserRole = userRole
            };
        }

        public static AuthenticationResult FailureResult(string errorMessage)
        {
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}