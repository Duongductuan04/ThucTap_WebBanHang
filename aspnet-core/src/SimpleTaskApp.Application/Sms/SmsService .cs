using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Sms
{
    public class SmsService : ISmsService
    {
        public Task SendSmsAsync(string phoneNumber, string message)
        {
            // TODO: Tích hợp SMS Gateway thực tế ở đây
            Console.WriteLine($"[SMS] Gửi tới {phoneNumber}: {message}");
            return Task.CompletedTask;
        }
    }
}
