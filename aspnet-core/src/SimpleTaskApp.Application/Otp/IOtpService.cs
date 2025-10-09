using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Otp
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string phoneNumber);
        bool VerifyOtp(string phoneNumber, string otpCode);
    }

}
