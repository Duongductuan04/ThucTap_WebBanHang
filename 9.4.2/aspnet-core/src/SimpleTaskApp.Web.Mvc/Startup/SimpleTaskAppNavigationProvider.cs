using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using SimpleTaskApp.Authorization;

namespace SimpleTaskApp.Web.Startup
{
    /// <summary>
    /// This class defines menus for the application.
    /// </summary>
    public class SimpleTaskAppNavigationProvider : NavigationProvider
    {
        public override void SetNavigation(INavigationProviderContext context)
        {
      context.Manager.MainMenu
        
          .AddItem(
              new MenuItemDefinition(
                  PageNames.Statistics,
                  L("Statistics"),
                  url: "/Admin/Statistics", // đường dẫn đến trang thống kê
                     icon: "fas fa-chart-bar",     // icon thống kê
                 permissionDependency: new SimplePermissionDependency(
                      PermissionNames.Pages_Statistics
                  ))
          )
          .AddItem(
          new MenuItemDefinition(
              "MobilePhonesAdmin",
              L("MobilePhones"),
              url: "/Admin/MobilePhones", // Backend DataTable
              icon: "fas fa-mobile-alt",
              permissionDependency: new SimplePermissionDependency(
                  PermissionNames.Pages_MobilePhone)

          )
      )
          .AddItem(
    new MenuItemDefinition(
        "MobilePhoneCategoryAdmin",   
        L("MobilePhoneCategories"),
       url: "/Admin/MobilePhoneCategories",
        icon: "fas fa-list",          // Icon danh mục
        permissionDependency: new SimplePermissionDependency(
            PermissionNames.Pages_MobilePhoneCategory
        )
    )
)
         .AddItem(
                  new MenuItemDefinition(
                      "ImportsAdmin",
                      L("Imports"),
                      url: "/Admin/Imports", // đường dẫn tới trang quản lý Import
                      icon: "fas fa-file-import",
                      permissionDependency: new SimplePermissionDependency(
                          PermissionNames.Pages_Import // 👈 nhớ khai báo permission cho Import
                      )
                  )
              )
               
           .AddItem(
              new MenuItemDefinition(
                  "DiscountsAdmin",
                  L("Discounts"),
                  url: "/Admin/Discounts", // đường dẫn tới trang quản lý Discounts
                  icon: "fas fa-tags",
                  permissionDependency: new SimplePermissionDependency(
                      PermissionNames.Pages_Discount // 👈 nhớ khai báo permission cho Discounts
                  )
              )
          ).AddItem(
              new MenuItemDefinition(
                  "OrdersAdmin",
                  L("Orders"),
                  url: "/Admin/Orders",
                  icon: "fas fa-box",
                  permissionDependency: new SimplePermissionDependency(
                      PermissionNames.Pages_Order
                  )
              )
          )
                    .AddItem(
              new MenuItemDefinition(
                  "ChatAdmin",              // tên định danh menu
                  L("CustomerSupport"), // hiển thị tên menu: Hỗ trợ khách hàng
                  url: "/Admin/Chats",       // đường dẫn tới controller/action chat admin
                  icon: "fas fa-comment",   // icon chat
                  permissionDependency: new SimplePermissionDependency(
                      PermissionNames.Pages_Chat // tạo permission riêng nếu muốn
                  )
              )
          ).AddItem(
                   new MenuItemDefinition(
                  PageNames.Tenants,
                  L("Tenants"),
                  url: "/Admin/Tenants",
                  icon: "fas fa-building",
                  permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
              )
          ).AddItem(
              new MenuItemDefinition(
                  PageNames.Users,
                  L("Users"),
                  url: "/Admin/Users",
                  icon: "fas fa-users",
                  permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Users)
              )
          ).AddItem(
              new MenuItemDefinition(
                  PageNames.Roles,
                  L("Roles"),
                  url: "/Admin/Roles",
                  icon: "fas fa-theater-masks",
                  permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
              )
          );
            
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, SimpleTaskAppConsts.LocalizationSourceName);
        }
    }
}