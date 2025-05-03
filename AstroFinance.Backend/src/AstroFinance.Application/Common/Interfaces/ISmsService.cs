using System.Threading.Tasks;

namespace AstroFinance.Application.Common.Interfaces
{
    public interface ISmsService
    {
        Task<SmsResult> SendSmsAsync(string phoneNumber, string message);
    }

    public class SmsResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? MessageId { get; set; }
    }
}