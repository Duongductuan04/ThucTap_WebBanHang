using Microsoft.AspNetCore.Mvc;
using Abp.Runtime.Session;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.MobilePhones.Dto;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;

public class UserOrdersController : AbpController
{
    private readonly IOrderAppService _orderAppService;
    private readonly IAbpSession _abpSession;

    public UserOrdersController(IOrderAppService orderAppService, IAbpSession abpSession)
    {
        _orderAppService = orderAppService;
        _abpSession = abpSession;
    }

    // Trang danh sách đơn hàng (có lọc theo trạng thái + phân trang)
    public async Task<IActionResult> Index(int? status, int page = 1, int pageSize = 15)
    {
        var userId = _abpSession.UserId;
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var request = new PagedOrderResultRequestDto
        {
            SkipCount = (page - 1) * pageSize,
            MaxResultCount = pageSize,
            UserId = userId.Value
        };

        // Chỉ lọc khi có trạng thái hợp lệ (0,1,2,...)
        if (status.HasValue && status.Value >= 0)
        {
            request.Status = status.Value;
        }

        var orders = await _orderAppService.GetAllAsync(request);

        // Truyền thông tin phân trang & filter sang View
        ViewBag.StatusValue = status ?? -1; // -1: tất cả
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = orders.TotalCount;

        return View(orders.Items);
    }

}
