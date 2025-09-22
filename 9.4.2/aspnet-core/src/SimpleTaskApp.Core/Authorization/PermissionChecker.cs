using Abp.Authorization;
using SimpleTaskApp.Authorization.Roles;
using SimpleTaskApp.Authorization.Users;

namespace SimpleTaskApp.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
