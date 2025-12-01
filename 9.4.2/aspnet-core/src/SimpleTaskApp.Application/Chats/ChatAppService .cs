using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.Chats.Dto;
using SimpleTaskApp.Authorization.Users;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleTaskApp.MobilePhones;

namespace SimpleTaskApp.Chats
{
  [AbpAuthorize] // Phân quyền nếu cần
  public class ChatAppService : ApplicationService, IChatAppService
  {
    private readonly IRepository<ChatMessage, long> _chatRepo;
    private readonly IRepository<User, long> _userRepository;

    public ChatAppService(
        IRepository<ChatMessage, long> chatRepo,
        IRepository<User, long> userRepository)
    {
      _chatRepo = chatRepo;
      _userRepository = userRepository;
    }

    // ================== LƯU TIN NHẮN MỚI ==================
    [UnitOfWork]
    public async Task SaveMessage(long userId, string sender, string message)
    {
      if (string.IsNullOrWhiteSpace(message))
        throw new UserFriendlyException("Message cannot be empty.");

      await _chatRepo.InsertAsync(new ChatMessage
      {
        UserId = userId,
        Sender = sender,
        Message = message
      });

      await CurrentUnitOfWork.SaveChangesAsync();
    }

    // ================== LẤY LỊCH SỬ CHAT ==================
    public async Task<List<ChatMessageDto>> GetHistory(long userId)
    {
      var messages = await _chatRepo.GetAll()
          .Where(x => x.UserId == userId)
          .OrderBy(x => x.CreationTime)
          .ToListAsync();

      return messages.Select(x => new ChatMessageDto
      {
        Id = x.Id,
        UserId = x.UserId,
        Sender = x.Sender,
        Message = x.Message,
        CreationTime = x.CreationTime
      }).ToList();
    }

    // ================== LẤY DANH SÁCH USER ĐÃ CHAT ==================
    public async Task<List<UserDto>> GetUsersWithChats()
    {
      // Lấy danh sách userId có chat
      var userIds = await _chatRepo.GetAll()
          .Select(x => x.UserId)
          .Distinct()
          .ToListAsync();

      // Lấy thông tin user
      var users = await _userRepository.GetAll()
          .Where(u => userIds.Contains(u.Id))
          .Select(u => new UserDto
          {
            Id = u.Id,
            UserName = u.UserName,
            LastMessage = _chatRepo.GetAll()
                  .Where(c => c.UserId == u.Id)
                  .OrderByDescending(c => c.CreationTime)
                  .Select(c => c.Message)
                  .FirstOrDefault()
          })
          .ToListAsync();

      return users;
    }


    // ================== XÓA LỊCH SỬ CHAT ==================
    [UnitOfWork]
    public async Task DeleteHistory(long userId)
    {
      var messages = await _chatRepo.GetAll()
          .Where(x => x.UserId == userId)
          .ToListAsync();

      if (messages.Any())
      {
        foreach (var msg in messages)
        {
          await _chatRepo.DeleteAsync(msg.Id);
        }
        await CurrentUnitOfWork.SaveChangesAsync();
      }
    }
  }
}
