using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Otp
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;

        public OtpService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<string> GenerateOtpAsync(string phoneNumber)
        {
            // Tạo OTP 6 chữ số
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP vào cache trong 5 phút
            _cache.Set($"OTP_{phoneNumber}", otp, TimeSpan.FromMinutes(5));

            return Task.FromResult(otp);
        }

        public bool VerifyOtp(string phoneNumber, string otpCode)
        {
            if (_cache.TryGetValue($"OTP_{phoneNumber}", out string cachedOtp))
            {
                if (cachedOtp == otpCode)
                {
                    _cache.Remove($"OTP_{phoneNumber}"); // Xóa OTP sau khi xác thực
                    return true;
                }
            }
            return false;
        }
    }
}
