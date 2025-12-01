using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.Chats;
using SimpleTaskApp.Chats.Dto;
using SimpleTaskApp.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTaskApp.Web.Areas.Admin.Controllers
{
  [Area("Admin")]
  [AbpMvcAuthorize(PermissionNames.Pages_Chat)] // Chỉ admin có quyền xem menu chat
  public class ChatsController : SimpleTaskAppControllerBase
  {
    private readonly IChatAppService _chatService;

    public ChatsController(IChatAppService chatService)
    {
      _chatService = chatService;
    }

    // GET: /Admin/Chat
    public IActionResult Index()
    {
      // Trang chính chat admin, load view
      return View();
    }

    // Lấy danh sách lịch sử chat của user
    [AbpMvcAuthorize(PermissionNames.Pages_Chat_ViewHistory)]
    public async Task<JsonResult> GetHistory(long userId)
    {
      var history = await _chatService.GetHistory(userId);
      return Json(history);
    }
    [AbpMvcAuthorize(PermissionNames.Pages_Chat_ViewHistory)]
    public async Task<JsonResult> GetUsers()
    {
      var users = await _chatService.GetUsersWithChats();
      return Json(users);
    }
    // Xóa lịch sử chat của user
    [AbpMvcAuthorize(PermissionNames.Pages_Chat_DeleteMessage)]
    public async Task<JsonResult> DeleteHistory(long userId)
    {
      await _chatService.DeleteHistory(userId);
      return Json(new { success = true });
    }
  }
}
