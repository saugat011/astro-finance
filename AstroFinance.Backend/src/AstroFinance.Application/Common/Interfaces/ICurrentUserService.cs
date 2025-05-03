namespace AstroFinance.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? UserRole { get; }
        bool IsAuthenticated { get; }
    }
}