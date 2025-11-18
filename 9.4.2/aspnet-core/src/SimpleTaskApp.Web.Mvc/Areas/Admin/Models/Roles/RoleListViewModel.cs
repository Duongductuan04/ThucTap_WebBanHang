using System.Collections.Generic;
using SimpleTaskApp.Roles.Dto;

namespace SimpleTaskApp.Areas.Admin.Models.Roles
{
    public class RoleListViewModel
    {
        public IReadOnlyList<PermissionDto> Permissions { get; set; }
    }
}
