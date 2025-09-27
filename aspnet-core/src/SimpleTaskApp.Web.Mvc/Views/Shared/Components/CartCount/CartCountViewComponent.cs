using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.MobilePhones.Dto;
using SimpleTaskApp.MobilePhones;
using System.Threading.Tasks;
using System.Linq;

public class CartCountViewComponent : ViewComponent
{
    private readonly ICartAppService _cartAppService;
    private readonly IAbpSession _abpSession;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public CartCountViewComponent(ICartAppService cartAppService, IAbpSession abpSession, IUnitOfWorkManager unitOfWorkManager)
    {
        _cartAppService = cartAppService;
        _abpSession = abpSession;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int count = 0;

        if (_abpSession.UserId.HasValue)
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                var carts = await _cartAppService.GetAllAsync(new PagedCartResultRequestDto
                {
                    UserId = _abpSession.UserId.Value,
                    MaxResultCount = 100
                });

                count = carts.Items.Count(); // Đây chỉ đếm số item, không cộng số lượng
                await uow.CompleteAsync();
            }
        }

        return View(count);
    }
}
