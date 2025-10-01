using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Castle.MicroKernel.Registration;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.Authorization.Users;
using SimpleTaskApp.MobilePhones.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    [AbpAuthorize]
    public class OrderAppService : SimpleTaskAppAppServiceBase, IOrderAppService
    {
        private readonly IRepository<Order, int> _orderRepository;
        private readonly IRepository<OrderDetail, int> _orderDetailRepository;
        private readonly IRepository<MobilePhone, int> _mobilePhoneRepository;
        private readonly IRepository<Discount, int> _discountRepository;

        public OrderAppService(
            IRepository<Order, int> orderRepository,
            IRepository<OrderDetail, int> orderDetailRepository,
            IRepository<MobilePhone, int> mobilePhoneRepository,
            IRepository<Discount, int> discountRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _mobilePhoneRepository = mobilePhoneRepository;
            _discountRepository = discountRepository;
        }

        // ================== TẠO ĐƠN HÀNG ==================
        public async Task<OrderDto> CreateAsync(CreateOrderDto input)
        {
            // Khởi tạo đơn hàng
            var order = new Order
            {
                UserId = AbpSession.UserId.Value,
                RecipientName = input.RecipientName,
                RecipientAddress = input.RecipientAddress,
                RecipientPhone = input.RecipientPhone,
                PaymentMethod = input.PaymentMethod,
                ShippingMethod = input.ShippingMethod,
                Status = input.Status,
                    Note = input.Note   // thêm ghi chú

            };

            // Insert order để sinh Id
            await _orderRepository.InsertAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Thêm chi tiết đơn hàng
            foreach (var od in input.OrderDetails)
            {
                var phone = await _mobilePhoneRepository.GetAsync(od.MobilePhoneId);

                var unitPrice = phone.DiscountPrice.HasValue && phone.DiscountPrice.Value > 0
                    ? phone.DiscountPrice.Value
                    : phone.Price;

                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    MobilePhoneId = od.MobilePhoneId,
                    Quantity = od.Quantity,
                    UnitPrice = unitPrice
                };

                await _orderDetailRepository.InsertAsync(orderDetail);
            }

            // Load lại order cùng OrderDetails để tính tổng
            order = await _orderRepository.GetAll()
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            // Tính tổng tiền sản phẩm
            order.TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);

            // Tính phí ship
            order.ShippingFee = CalculateShippingFee(order.ShippingMethod);

            // Tổng cuối cùng
            order.FinalAmount = order.TotalAmount - order.DiscountAmount + order.ShippingFee;

            await _orderRepository.UpdateAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapToOrderDto(order);
        }


        // ================== LẤY ĐƠN HÀNG THEO ID ==================
        public async Task<OrderDto> GetAsync(EntityDto<int> input)
        {
            var order = await _orderRepository.GetAll()
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MobilePhone)
                .Include(o => o.Discount)
                .FirstOrDefaultAsync(o => o.Id == input.Id);

            if (order == null)
                throw new UserFriendlyException($"Order not found! Id = {input.Id}");

            return MapToOrderDto(order);
        }

        // ================== CẬP NHẬT ĐƠN HÀNG ==================
        public async Task<OrderDto> UpdateAsync(UpdateOrderDto input)
        {
            var order = await _orderRepository.GetAll()
                .Include(o => o.OrderDetails)
                .Include(o => o.Discount)
                .FirstOrDefaultAsync(o => o.Id == input.Id);

            if (order == null)
                throw new UserFriendlyException($"Order not found! Id = {input.Id}");

            order.RecipientName = input.RecipientName;
            order.RecipientAddress = input.RecipientAddress;
            order.RecipientPhone = input.RecipientPhone;
            order.PaymentMethod = input.PaymentMethod;
            order.ShippingMethod = input.ShippingMethod;
            order.Status = input.Status;
            order.Note= input.Note;

            // Cập nhật phí ship và tổng cuối
            order.ShippingFee = CalculateShippingFee(order.ShippingMethod);
            order.FinalAmount = order.TotalAmount - order.DiscountAmount + order.ShippingFee;

            await _orderRepository.UpdateAsync(order);
            return MapToOrderDto(order);
        }

        // ================== PHÂN TRANG ==================
        public async Task<PagedResultDto<OrderDto>> GetAllAsync(PagedOrderResultRequestDto input)
        {
            var query = _orderRepository.GetAll()
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MobilePhone)
                .Include(o => o.Discount)
                .AsQueryable();

            if (input.UserId.HasValue)
                query = query.Where(o => o.UserId == input.UserId.Value);

            if (input.Status.HasValue && input.Status.Value >= 0)
                query = query.Where(o => o.Status == input.Status.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<OrderDto>(totalCount, items.Select(MapToOrderDto).ToList());
        }

        // ================== XÓA ĐƠN HÀNG ==================
        public async Task DeleteAsync(EntityDto<int> input)
        {
            var order = await _orderRepository.GetAllIncluding(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == input.Id);

            if (order == null)
                throw new UserFriendlyException($"Order not found! Id = {input.Id}");

            if (order.OrderDetails != null)
            {
                foreach (var od in order.OrderDetails)
                    await _orderDetailRepository.DeleteAsync(od.Id);
            }

            await _orderRepository.DeleteAsync(order.Id);
        }

        // ================== PRIVATE: MAPPING ==================
        private OrderDto MapToOrderDto(Order order)
        {
            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                RecipientName = order.RecipientName,
                RecipientAddress = order.RecipientAddress,
                RecipientPhone = order.RecipientPhone,
                PaymentMethod = order.PaymentMethod,
                ShippingMethod = order.ShippingMethod,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                CreationTime = order.CreationTime,
                OrderDetails = order.OrderDetails?.Select(MapToOrderDetailDto).ToList(),
                Note = order.Note,
                // Thông tin voucher
                DiscountId = order.DiscountId,
                DiscountCode = order.Discount?.Code,
                DiscountPercentage = order.Discount?.Percentage
            };
        }

        private OrderDetailDto MapToOrderDetailDto(OrderDetail od)
        {
            if (od == null) return null;

            return new OrderDetailDto
            {
                Id = od.Id,
                OrderId = od.OrderId,
                MobilePhoneId = od.MobilePhoneId,
                MobilePhoneName = od.MobilePhone?.Name,
                ImageUrl = od.MobilePhone?.ImageUrl,
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice,
            };
        }

        // ================== PRIVATE: TÍNH PHÍ SHIP ==================
        private decimal CalculateShippingFee(int shippingMethod)
        {
            return shippingMethod switch
            {
                0 => 20000m, // Tiêu chuẩn
                1 => 40000m, // Nhanh
                2 => 60000m, // Siêu tốc
                _ => 0m
            };
        }
    }
}
