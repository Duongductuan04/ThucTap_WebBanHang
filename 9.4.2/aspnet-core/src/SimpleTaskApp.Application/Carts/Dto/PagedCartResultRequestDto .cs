using Abp.Application.Services.Dto;

namespace SimpleTaskApp.MobilePhones.Dto
{
    public class PagedCartResultRequestDto : PagedAndSortedResultRequestDto
    {
        public long? UserId { get; set; } // filter theo UserId
    }
}
