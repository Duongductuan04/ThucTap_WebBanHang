using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using SimpleTaskApp.Users;
using SimpleTaskApp.Users.Dto;
using System.Threading.Tasks;
using SimpleTaskApp.Controllers;

namespace SimpleTaskApp.Web.Controllers
{
    [AbpMvcAuthorize] // Chỉ user đã đăng nhập mới được đổi mật khẩu
    public class UsersController : SimpleTaskAppControllerBase
    {
        private readonly IUserAppService _userAppService;

        public UsersController(IUserAppService userAppService)
        {
            _userAppService = userAppService;
        }

        // GET: Hiển thị trang đổi mật khẩu
        public IActionResult ChangePassword()
        {
            return View();
        }

    }
}