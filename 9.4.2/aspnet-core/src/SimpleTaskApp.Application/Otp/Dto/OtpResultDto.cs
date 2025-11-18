using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Otp.Dto
{

    public class OtpResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OtpCode { get; set; }
    }
}
