using AstroFinance.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace AstroFinance.Infrastructure.Sms
{
    public class SmsService : ISmsService
    {
        public SmsService()
        {
            // Initialize SMS service provider here if needed
        }

     public Task<SmsResult> SendSmsAsync(string phoneNumber, string message)
{
    Console.WriteLine($"Sending SMS to {phoneNumber}: {message}");
    
    var result = new SmsResult
    {
        Success = true,
        MessageId = Guid.NewGuid().ToString(), // or null if you don't have one
        ErrorMessage = null
    };

    return Task.FromResult(result);
}


    }
}
