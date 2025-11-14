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
                  PageNames.About,
                  L("About"),
                  url: "/Admin/About",
                  icon: "fas fa-info-circle"
              )
          )
          .AddItem(
              new MenuItemDefinition(
                  PageNames.Home,
                  L("HomePage"),
                  url: "/Admin/Home",
                  icon: "fas fa-home",
                  requiresAuthentication: true
              )
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
                  "CartsAdmin",
                  L("Carts"),
                  url: "/Admin/Carts", // đường dẫn tới trang quản lý giỏ hàng
                  icon: "fas fa-shopping-cart",
                  permissionDependency: new SimplePermissionDependency(
                      PermissionNames.Pages_Cart // 👈 nhớ khai báo permission cho Cart
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
                  "StatisticsAdmin",
                  L("Statistics"),
                  url: "/Admin/Statistics", // đường dẫn đến trang thống kê
                  icon: "fas fa-chart-line",
                  permissionDependency: new SimplePermissionDependency(
                      PermissionNames.Pages_Statistics
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