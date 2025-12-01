using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Chats.Dto
{
  public class ChatMessageDto
  {
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Sender { get; set; } // "User" hoặc "Admin"
    public string Message { get; set; }
    public DateTime CreationTime { get; set; }
  }
  public class UserDto
  {
    public long Id { get; set; }
    public string UserName { get; set; }
    public string LastMessage { get; set; } // thêm dòng này
  }
}
