using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Sms
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }

}
