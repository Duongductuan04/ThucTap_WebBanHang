using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Otp.Dto
{

    public class VerifyOtpDto
    {
        public string PhoneNumber { get; set; }
        public string OtpCode { get; set; }
    }
}
