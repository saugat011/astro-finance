using AstroFinance.Application.Common.Models;

namespace AstroFinance.Application.Common.Interfaces
{
    public interface ISmsService
    {
        Task<Result> SendSmsAsync(string phoneNumber, string message);
        Task<Result> SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message);
    }
}