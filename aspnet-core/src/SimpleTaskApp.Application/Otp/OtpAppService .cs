using Abp.Application.Services;
using SimpleTaskApp.Otp.Dto;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SimpleTaskApp.Otp
{
    public class OtpAppService : ApplicationService, IOtpAppService
    {
        private static readonly ConcurrentDictionary<string, OtpDataDto> _otpStorage = new ConcurrentDictionary<string, OtpDataDto>();
        private readonly TimeSpan _otpExpiration = TimeSpan.FromMinutes(5);

        public Task<OtpResultDto> SendOtpAsync(SendOtpDto input)
        {
            try
            {
                var normalizedPhone = NormalizePhoneNumber(input.PhoneNumber);
                var otpCode = GenerateOtp();

                _otpStorage[normalizedPhone] = new OtpDataDto
                {
                    OtpCode = otpCode,
                    ExpirationTime = DateTime.UtcNow.Add(_otpExpiration),
                    Attempts = 0
                };

                // Log OTP cho test
                System.Diagnostics.Debug.WriteLine($"OTP FOR {normalizedPhone}: {otpCode}");

                return Task.FromResult(new OtpResultDto
                {
                    Success = true,
                    Message = "Mã OTP đã được gửi đến số điện thoại của bạn",
                    OtpCode = otpCode // chỉ dùng dev/test
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OtpResultDto
                {
                    Success = false,
                    Message = $"Lỗi khi gửi OTP: {ex.Message}"
                });
            }
        }

        public Task<OtpResultDto> VerifyOtpAsync(VerifyOtpDto input)
        {
            try
            {
                var normalizedPhone = NormalizePhoneNumber(input.PhoneNumber);

                if (!_otpStorage.TryGetValue(normalizedPhone, out var otpData))
                {
                    return Task.FromResult(new OtpResultDto
                    {
                        Success = false,
                        Message = "Mã OTP không tồn tại hoặc đã hết hạn"
                    });
                }

                if (DateTime.UtcNow > otpData.ExpirationTime)
                {
                    _otpStorage.TryRemove(normalizedPhone, out _);
                    return Task.FromResult(new OtpResultDto
                    {
                        Success = false,
                        Message = "Mã OTP đã hết hạn"
                    });
                }

                if (otpData.Attempts >= 3)
                {
                    _otpStorage.TryRemove(normalizedPhone, out _);
                    return Task.FromResult(new OtpResultDto
                    {
                        Success = false,
                        Message = "Đã vượt quá số lần thử cho phép"
                    });
                }

                otpData.Attempts++;

                if (otpData.OtpCode == input.OtpCode)
                {
                    return Task.FromResult(new OtpResultDto
                    {
                        Success = true,
                        Message = "Xác thực OTP thành công"
                    });
                }

                _otpStorage[normalizedPhone] = otpData;
                return Task.FromResult(new OtpResultDto
                {
                    Success = false,
                    Message = "Mã OTP không chính xác"
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OtpResultDto
                {
                    Success = false,
                    Message = $"Lỗi khi xác thực OTP: {ex.Message}"
                });
            }
        }

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            return phoneNumber?.Replace(" ", "").Replace("+", "").Trim();
        }
    }
}
