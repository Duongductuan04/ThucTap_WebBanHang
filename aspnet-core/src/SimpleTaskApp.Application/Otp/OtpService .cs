using Abp.Application.Services;
using SimpleTaskApp.Otp.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Otp
{


    public class OtpService : ApplicationService, IOtpService
    {
        // Lưu trữ OTP tạm thời trong memory (có thể thay bằng Redis trong production)
        private static readonly ConcurrentDictionary<string, OtpData> _otpStorage = new ConcurrentDictionary<string, OtpData>();
        private readonly TimeSpan _otpExpiration = TimeSpan.FromMinutes(5); // OTP hết hạn sau 5 phút

        public Task<OtpResult> SendOtpAsync(string phoneNumber)
        {
            try
            {
                // Chuẩn hóa số điện thoại
                var normalizedPhone = NormalizePhoneNumber(phoneNumber);

                // Tạo mã OTP ngẫu nhiên 6 chữ số
                var otpCode = GenerateOtp();

                // Lưu OTP vào storage
                var otpData = new OtpData
                {
                    OtpCode = otpCode,
                    ExpirationTime = DateTime.UtcNow.Add(_otpExpiration),
                    Attempts = 0
                };

                _otpStorage[normalizedPhone] = otpData;

                // Giả lập gửi SMS - Trong thực tế sẽ tích hợp với dịch vụ SMS
                Console.WriteLine($"DEBUG: OTP {otpCode} đã được gửi đến số {normalizedPhone}");

                // Trong môi trường development, có thể log OTP để dễ test
                System.Diagnostics.Debug.WriteLine($"OTP FOR {normalizedPhone}: {otpCode}");

                return Task.FromResult(new OtpResult
                {
                    Success = true,
                    Message = "Mã OTP đã được gửi đến số điện thoại của bạn",
                    OtpCode = otpCode // Chỉ trả về trong môi trường test
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OtpResult
                {
                    Success = false,
                    Message = $"Lỗi khi gửi OTP: {ex.Message}"
                });
            }
        }

        public Task<OtpResult> VerifyOtpAsync(string phoneNumber, string otpCode)
        {
            try
            {
                var normalizedPhone = NormalizePhoneNumber(phoneNumber);

                if (!_otpStorage.TryGetValue(normalizedPhone, out var otpData))
                {
                    return Task.FromResult(new OtpResult
                    {
                        Success = false,
                        Message = "Mã OTP không tồn tại hoặc đã hết hạn"
                    });
                }

                // Kiểm tra hết hạn
                if (DateTime.UtcNow > otpData.ExpirationTime)
                {
                    _otpStorage.TryRemove(normalizedPhone, out _);
                    return Task.FromResult(new OtpResult
                    {
                        Success = false,
                        Message = "Mã OTP đã hết hạn"
                    });
                }

                // Kiểm tra số lần thử
                if (otpData.Attempts >= 3)
                {
                    _otpStorage.TryRemove(normalizedPhone, out _);
                    return Task.FromResult(new OtpResult
                    {
                        Success = false,
                        Message = "Đã vượt quá số lần thử cho phép"
                    });
                }

                // Tăng số lần thử
                otpData.Attempts++;

                // Kiểm tra mã OTP
                if (otpData.OtpCode == otpCode)
                {
                    // Xóa OTP sau khi xác thực thành công
                    _otpStorage.TryRemove(normalizedPhone, out _);
                    return Task.FromResult(new OtpResult
                    {
                        Success = true,
                        Message = "Xác thực OTP thành công"
                    });
                }
                else
                {
                    _otpStorage[normalizedPhone] = otpData;
                    return Task.FromResult(new OtpResult
                    {
                        Success = false,
                        Message = "Mã OTP không chính xác"
                    });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OtpResult
                {
                    Success = false,
                    Message = $"Lỗi khi xác thực OTP: {ex.Message}"
                });
            }
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            // Chuẩn hóa số điện thoại: loại bỏ khoảng trắng, dấu +
            return phoneNumber?.Replace(" ", "").Replace("+", "").Trim();
        }
    }
}
