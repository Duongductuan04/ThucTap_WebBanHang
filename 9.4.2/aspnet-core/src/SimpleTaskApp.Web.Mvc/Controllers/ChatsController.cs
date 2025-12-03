using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.Chats;
using SimpleTaskApp.Controllers;
using System.Threading.Tasks;

namespace SimpleTaskApp.Web.Controllers
{
  public class ChatsController : SimpleTaskAppControllerBase
  {
    private readonly IChatAppService _chatService;

    public ChatsController(IChatAppService chatService)
    {
      _chatService = chatService;
    }

    [HttpGet]
    public async Task<JsonResult> GetHistory(long userId)
    {
      var history = await _chatService.GetHistory(userId);
      return Json(history);
    }
  }
}
