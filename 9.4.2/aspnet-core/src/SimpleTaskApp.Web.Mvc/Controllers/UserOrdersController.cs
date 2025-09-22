using Microsoft.AspNetCore.Mvc;
using Abp.Runtime.Session;
using SimpleTaskApp.MobilePhones;
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

public async Task<IActionResult> Index(int? status)
{
    var userId = _abpSession.UserId;
    if (userId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    var orders = await _orderAppService.GetOrdersByUserAndStatusAsync(userId.Value, status);
    ViewBag.StatusValue = status ?? -1; // mặc định tất cả
    return View(orders);
}

}
