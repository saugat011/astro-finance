using AstroFinance.Application.Common.Interfaces;
using AstroFinance.Application.Common.Models;
using AstroFinance.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace AstroFinance.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly IApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IDateTime _dateTime;

        public IdentityService(
            IApplicationDbContext context,
            IConfiguration configuration,
            IDateTime dateTime)
        {
            _context = context;
            _configuration = configuration;
            _dateTime = dateTime;
        }

        public async Task<string?> GetUserNameAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            return user?.Email;
        }

        public async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            return user != null && user.Role == role;
        }

        public async Task<bool> AuthorizeAsync(string userId, string policyName)
        {
            // For simplicity, we'll just check if the user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            return user != null;
        }

        public async Task<(Result Result, string UserId)> CreateUserAsync(string email, string password)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                return (Result.Failure("User with this email already exists."), string.Empty);
            }

            var user = new User
            {
                Email = email,
                PasswordHash = BC.HashPassword(password),
                Role = "User", // Default role
                IsActive = true,
                FirstName = "New",
                LastName = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(default);

            return (Result.Success(), user.Id.ToString());
        }

        public async Task<Result> DeleteUserAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            user.IsActive = false;
            await _context.SaveChangesAsync(default);

            return Result.Success();
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return AuthenticationResult.FailureResult("User not found.");
            }

            if (!user.IsActive)
            {
                return AuthenticationResult.FailureResult("User is inactive.");
            }

            if (!BC.Verify(password, user.PasswordHash))
            {
                return AuthenticationResult.FailureResult("Invalid password.");
            }

            // Update last login date
            user.LastLoginDate = _dateTime.Now;
            await _context.SaveChangesAsync(default);

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiryMinutes = _configuration.GetSection("JwtSettings:ExpiryMinutes").Get<int>();
            var expiresAt = _dateTime.Now.AddMinutes(expiryMinutes);

            return AuthenticationResult.SuccessResult(
                token,
                refreshToken,
                expiresAt,
                user.Id.ToString(),
                $"{user.FirstName} {user.LastName}",
                user.Role
            );
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = _dateTime.Now.AddMinutes(expiryMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}