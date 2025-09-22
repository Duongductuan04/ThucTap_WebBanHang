using System.Collections.Generic;
using SimpleTaskApp.Roles.Dto;

namespace SimpleTaskApp.Areas.Admin.Models.Users
{
    public class UserListViewModel
    {
        public IReadOnlyList<RoleDto> Roles { get; set; }
    }
}
