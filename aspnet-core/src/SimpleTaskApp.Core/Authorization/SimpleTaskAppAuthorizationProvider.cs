using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace SimpleTaskApp.Authorization
{
    public class SimpleTaskAppAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

            // Permissions cho Điện thoại
            var mobile = context.CreatePermission(PermissionNames.Pages_MobilePhone, L("MobilePhone"));
            mobile.CreateChildPermission(PermissionNames.Pages_MobilePhone_Create, L("CreateMobilePhone"));
            mobile.CreateChildPermission(PermissionNames.Pages_MobilePhone_Edit, L("EditMobilePhone"));
            mobile.CreateChildPermission(PermissionNames.Pages_MobilePhone_Delete, L("DeleteMobilePhone"));
            
            // Permissions cho Giỏ Hàng
            var cart = context.CreatePermission(PermissionNames.Pages_Cart, L("Cart"));
            mobile.CreateChildPermission(PermissionNames.Pages_Cart_Create, L("CreateCart"));

            mobile.CreateChildPermission(PermissionNames.Pages_Cart_Edit, L("EditCart"));
            mobile.CreateChildPermission(PermissionNames.Pages_Cart_Delete, L("DeleteCart"));
            // THÊM PERMISSIONS CHO DANH MỤC ĐIỆN THOẠI - THÊM VÀO ĐÂY
            var mobilePhoneCategory = context.CreatePermission(PermissionNames.Pages_MobilePhoneCategory, L("MobilePhoneCategory"));
            mobilePhoneCategory.CreateChildPermission(PermissionNames.Pages_MobilePhoneCategory_Create, L("CreateMobilePhoneCategory"));
            mobilePhoneCategory.CreateChildPermission(PermissionNames.Pages_MobilePhoneCategory_Edit, L("EditMobilePhoneCategory"));
            mobilePhoneCategory.CreateChildPermission(PermissionNames.Pages_MobilePhoneCategory_Delete, L("DeleteMobilePhoneCategory"));
            var order = context.CreatePermission(PermissionNames.Pages_Order, L("Order"));
            order.CreateChildPermission(PermissionNames.Pages_Order_Create, L("CreateOrder"));
            order.CreateChildPermission(PermissionNames.Pages_Order_Edit, L("EditOrder"));
            order.CreateChildPermission(PermissionNames.Pages_Order_Delete, L("DeleteOrder"));

            // Permissions cho Discounts
            var discount = context.CreatePermission(PermissionNames.Pages_Discount, L("Discount"));
            discount.CreateChildPermission(PermissionNames.Pages_Discount_Create, L("CreateDiscount"));
            discount.CreateChildPermission(PermissionNames.Pages_Discount_Edit, L("EditDiscount"));
            discount.CreateChildPermission(PermissionNames.Pages_Discount_Delete, L("DeleteDiscount"));


            var statistics = context.CreatePermission(PermissionNames.Pages_Statistics, L("Statistics"));

        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, SimpleTaskAppConsts.LocalizationSourceName);
        }
    }
}