using SimpleTaskApp.Otp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Otp
{
    public interface IOtpService
    {
        Task<OtpResult> SendOtpAsync(string phoneNumber);
        Task<OtpResult> VerifyOtpAsync(string phoneNumber, string otpCode);
    }
}
