using Microsoft.AspNetCore.SignalR;
using SimpleTaskApp.Chats;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using Abp.Authorization;

namespace SimpleTaskApp.Web.Hubs
{
  [AbpAuthorize] // admin hoặc user đều cần login
  public class ChatHub : Hub
  {
    private static ConcurrentDictionary<long, string> ConnectedUsers = new(); // UserId -> ConnectionId
    private readonly IChatAppService _chatService;

    public ChatHub(IChatAppService chatService)
    {
      _chatService = chatService;
    }

    [UnitOfWork] // commit vào DB
    public async Task SendMessageToAdmin(long userId, string message)
    {
      // Lưu vào DB
      await _chatService.SaveMessage(userId, "User", message);

      // Gửi tới tất cả admin
      await Clients.Group("Admins").SendAsync("ReceiveMessage", userId, message, "User");
    }

    [UnitOfWork]
    public async Task SendMessageToUser(long userId, string message)
    {
      await _chatService.SaveMessage(userId, "Admin", message);

      // Gửi tới user nếu đang online
      if (ConnectedUsers.TryGetValue(userId, out var connectionId))
      {
        await Clients.Client(connectionId).SendAsync("ReceiveMessage", "Admin", message, "Admin");
      }
    }

    // Khi user kết nối
    public override async Task OnConnectedAsync()
    {
      var http = Context.GetHttpContext();
      var role = http.Request.Query["role"].ToString();

      if (role == "admin")
      {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
      }
      else
      {
        // giả sử userId gửi kèm query string ?userId=123
        if (long.TryParse(http.Request.Query["userId"], out long userId))
        {
          ConnectedUsers[userId] = Context.ConnectionId;
        }
      }

      await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(System.Exception? exception)
    {
      // Xóa khỏi dictionary nếu user
      var user = ConnectedUsers.FirstOrDefault(x => x.Value == Context.ConnectionId);
      if (user.Key != 0)
        ConnectedUsers.TryRemove(user.Key, out _);

      await base.OnDisconnectedAsync(exception);
    }
  }
}
