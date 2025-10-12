using SimpleTaskApp.Otp.Dto;
using System.Threading.Tasks;

namespace SimpleTaskApp.Otp
{
    public interface IOtpAppService
    {
        Task<OtpResultDto> SendOtpAsync(SendOtpDto input);
        Task<OtpResultDto> VerifyOtpAsync(VerifyOtpDto input);
    }
}
