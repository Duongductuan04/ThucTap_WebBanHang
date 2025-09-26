using System.Collections.Generic;
using SimpleTaskApp.Roles.Dto;

namespace SimpleTaskApp.Web.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }
    }
}