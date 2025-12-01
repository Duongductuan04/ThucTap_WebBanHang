using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using SimpleTaskApp.Authorization.Users; // namespace User của bạn

namespace SimpleTaskApp.MobilePhones
{
  [Table("AppChatMessages")]
  public class ChatMessage : Entity<long>, IHasCreationTime
  {

    public long UserId { get; set; }


    public string Sender { get; set; }

    public string Message { get; set; }


    public DateTime CreationTime { get; set; } = Clock.Now;


    [ForeignKey("UserId")]
    public virtual User User { get; set; }
  }
}
