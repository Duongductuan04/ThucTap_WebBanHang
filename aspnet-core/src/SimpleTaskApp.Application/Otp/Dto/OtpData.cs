using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Otp.Dto
{
    public class OtpData
    {
        public string OtpCode { get; set; }
        public DateTime ExpirationTime { get; set; }
        public int Attempts { get; set; }
    }
}
