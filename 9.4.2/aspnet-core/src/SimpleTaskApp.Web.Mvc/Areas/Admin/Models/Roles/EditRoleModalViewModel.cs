using Abp.AutoMapper;
using SimpleTaskApp.Roles.Dto;
using SimpleTaskApp.Areas.Admin.Models.Common;

namespace SimpleTaskApp.Areas.Admin.Models.Roles
{
    [AutoMapFrom(typeof(GetRoleForEditOutput))]
    public class EditRoleModalViewModel : GetRoleForEditOutput, IPermissionsEditViewModel
    {
        public bool HasPermission(FlatPermissionDto permission)
        {
            return GrantedPermissionNames.Contains(permission.Name);
        }
    }
}
