using Abp.AutoMapper;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMapTo(typeof(Cart))]
    public class CreateCartDto
    {
        public long UserId { get; set; }
        public int MobilePhoneId { get; set; }
        public int Quantity { get; set; } = 1;
        public int? MobilePhoneColorId { get; set; } // ✅ thêm dòng này

  }
}
