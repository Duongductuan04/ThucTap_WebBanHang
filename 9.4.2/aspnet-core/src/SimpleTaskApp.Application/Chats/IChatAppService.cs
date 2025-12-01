using Abp.Application.Services;
using SimpleTaskApp.Chats.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTaskApp.Chats
{
  public interface IChatAppService : IApplicationService
  {
    // Lưu tin nhắn mới
    Task SaveMessage(long userId, string sender, string message);

    // Lấy lịch sử chat của 1 user
    Task<List<ChatMessageDto>> GetHistory(long userId);

    // Xóa toàn bộ lịch sử chat của 1 user
    Task DeleteHistory(long userId);

    // Lấy danh sách user đã từng chat
    Task<List<UserDto>> GetUsersWithChats();
  }
}
