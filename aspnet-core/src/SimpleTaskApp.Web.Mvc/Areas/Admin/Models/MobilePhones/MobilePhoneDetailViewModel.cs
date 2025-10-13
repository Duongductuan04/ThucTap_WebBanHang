using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;

namespace SimpleTaskApp.Areas.Admin.Models.MobilePhones
{
    public class MobilePhoneDetailViewModel
    {
        public MobilePhoneDto MobilePhone { get; set; }
        public List<MobilePhoneColorDto> Colors { get; set; }
    }
}
