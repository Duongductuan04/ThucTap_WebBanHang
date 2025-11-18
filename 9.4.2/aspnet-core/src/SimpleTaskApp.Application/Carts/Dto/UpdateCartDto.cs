using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMap(typeof(Cart))]
    public class UpdateCartDto : EntityDto<int>
    {
        public int Quantity { get; set; }
    }
}
