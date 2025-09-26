using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    [AbpAuthorize] // bắt buộc đăng nhập 
    public class OrderAppService : SimpleTaskAppAppServiceBase, IOrderAppService
    {
        private readonly IRepository<Order, int> _orderRepository;
        private readonly IRepository<OrderDetail, int> _orderDetailRepository;
        private readonly IRepository<MobilePhone, int> _mobilePhoneRepository;

        public OrderAppService(
            IRepository<Order, int> orderRepository,
            IRepository<OrderDetail, int> orderDetailRepository,
            IRepository<MobilePhone, int> mobilePhoneRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _mobilePhoneRepository = mobilePhoneRepository;
        }

        // Tạo đơn hàng
        public async Task<OrderDto> CreateAsync(CreateOrderDto input)
        {
            var order = new Order
            {
                UserId = AbpSession.UserId.Value,
                RecipientName = input.RecipientName,
                RecipientAddress = input.RecipientAddress,
                RecipientPhone = input.RecipientPhone,
                PaymentMethod = input.PaymentMethod,
                ShippingMethod = input.ShippingMethod,
                Status = input.Status
            };

            // Thêm order trước, SaveChanges để sinh Id
            await _orderRepository.InsertAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync(); // 👈 Quan trọng

            // Thêm chi tiết đơn hàng
            foreach (var od in input.OrderDetails)
            {
                var phone = await _mobilePhoneRepository.GetAsync(od.MobilePhoneId);

                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id, // giờ đã có Id thật
                    MobilePhoneId = od.MobilePhoneId,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                };

                await _orderDetailRepository.InsertAsync(orderDetail);

                if (order.OrderDetails == null)
                    order.OrderDetails = new List<OrderDetail>();

                order.OrderDetails.Add(orderDetail);
            }

            // Cập nhật tổng tiền
            order.TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);

            await _orderRepository.UpdateAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync(); // lưu tổng tiền

            return MapToOrderDto(order);
        }

        // Lấy đơn hàng theo Id
        public async Task<OrderDto> GetAsync(EntityDto<int> input)
        {
            var order = await _orderRepository.GetAll()
     .Include(o => o.OrderDetails)
     .ThenInclude(od => od.MobilePhone)
     .FirstOrDefaultAsync(o => o.Id == input.Id);


            if (order == null)
                throw new UserFriendlyException($"Order not found! Id = {input.Id}");

            return MapToOrderDto(order);
        }

        // Cập nhật đơn hàng
        public async Task<OrderDto> UpdateAsync(UpdateOrderDto input)
        {
            var order = await _orderRepository.GetAll()
                           .Include(o => o.OrderDetails)                     // load OrderDetails
                           .FirstOrDefaultAsync(o => o.Id == input.Id);

            if (order == null)
                throw new UserFriendlyException($"Order not found! Id = {input.Id}");

            order.RecipientName = input.RecipientName;
            order.RecipientAddress = input.RecipientAddress;
            order.RecipientPhone = input.RecipientPhone;
            order.PaymentMethod = input.PaymentMethod;
            order.ShippingMethod = input.ShippingMethod;
            order.Status = input.Status;

            await _orderRepository.UpdateAsync(order);

            return MapToOrderDto(order);
        }

        // Lấy danh sách phân trang đơn hàng
        public async Task<PagedResultDto<OrderDto>> GetAllAsync(PagedOrderResultRequestDto input)
        {
            var query = _orderRepository.GetAllIncluding(o => o.OrderDetails);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtoList = items.Select(MapToOrderDto).ToList();

            return new PagedResultDto<OrderDto>(totalCount, dtoList);
        }

        // Xóa đơn hàng theo Id
        public async Task DeleteAsync(EntityDto<int> input)
        {
            var order = await _orderRepository.GetAllIncluding(o => o.OrderDetails)
                                              .FirstOrDefaultAsync(o => o.Id == input.Id);

            if (order == null)
                throw new UserFriendlyException($"Order not found! Id = {input.Id}");

            // Xóa chi tiết đơn hàng trước
            if (order.OrderDetails != null)
            {
                foreach (var od in order.OrderDetails)
                {
                    await _orderDetailRepository.DeleteAsync(od.Id);
                }
            }

            // Xóa đơn hàng
            await _orderRepository.DeleteAsync(order.Id);
        }
        public async Task<List<OrderDto>> GetOrdersByUserAndStatusAsync(long userId, int? status)
        {
            var query = _orderRepository
                .GetAll()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MobilePhone)
                .Where(o => o.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreationTime)
                .ToListAsync();

            return orders.Select(MapToOrderDto).ToList();
        }

        // Hàm map Order -> OrderDto
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
                CreationTime = order.CreationTime,
                OrderDetails = order.OrderDetails?.Select(MapToOrderDetailDto).ToList()
            };
        }

        // Hàm map OrderDetail -> OrderDetailDto
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
    }
}
