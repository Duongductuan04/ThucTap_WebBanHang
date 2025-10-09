using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using SimpleTaskApp.Authorization.Roles;
using SimpleTaskApp.Authorization.Users;
using SimpleTaskApp.MultiTenancy;
using SimpleTaskApp.MobilePhones;

namespace SimpleTaskApp.EntityFrameworkCore
{
    public class SimpleTaskAppDbContext : AbpZeroDbContext<Tenant, Role, User, SimpleTaskAppDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public DbSet<MobilePhoneCategory> MobilePhoneCategories { get; set; }
        public DbSet<MobilePhone> MobilePhones { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        // ==== Thêm bảng voucher / discount ====
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountCategory> DiscountCategories { get; set; }
        public DbSet<DiscountProduct> DiscountProducts { get; set; }
        public DbSet<Import> Imports { get; set; }
        public DbSet<ImportDetail> ImportDetails { get; set; }
        public SimpleTaskAppDbContext(DbContextOptions<SimpleTaskAppDbContext> options)
            : base(options)
        {
        }
    }
}
