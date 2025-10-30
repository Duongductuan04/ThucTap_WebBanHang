using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
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
    private readonly IRepository<DiscountCategory, int> _discountCategoryRepository;
    private readonly IRepository<DiscountProduct, int> _discountProductRepository;
    private readonly IRepository<MobilePhoneColor, int> _colorRepository;


    public OrderAppService(
        IRepository<Order, int> orderRepository,
        IRepository<OrderDetail, int> orderDetailRepository,
        IRepository<MobilePhone, int> mobilePhoneRepository,
            IRepository<MobilePhoneColor, int> colorRepository,

        IRepository<Discount, int> discountRepository,
        IRepository<DiscountCategory, int> discountCategoryRepository,
        IRepository<DiscountProduct, int> discountProductRepository)
    {
      _orderRepository = orderRepository;
      _orderDetailRepository = orderDetailRepository;
      _mobilePhoneRepository = mobilePhoneRepository;
      _colorRepository = colorRepository;

      _discountRepository = discountRepository;
      _discountCategoryRepository = discountCategoryRepository;
      _discountProductRepository = discountProductRepository;
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
        Note = input.Note
      };

      // Insert order để sinh Id
      await _orderRepository.InsertAsync(order);
      await CurrentUnitOfWork.SaveChangesAsync();

      // Thêm chi tiết đơn hàng và giảm tồn kho
      foreach (var od in input.OrderDetails)
      {
        var phone = await _mobilePhoneRepository.GetAsync(od.MobilePhoneId);
        var color = od.MobilePhoneColorId.HasValue
       ? await _colorRepository.FirstOrDefaultAsync(c => c.Id == od.MobilePhoneColorId.Value)
       : null;
        // Kiểm tra tồn kho
        if (phone.StockQuantity < od.Quantity)
        {
          throw new UserFriendlyException($"Sản phẩm {phone.Name} chỉ còn {phone.StockQuantity} trong kho");
        }

        var unitPrice = phone.DiscountPrice.HasValue && phone.DiscountPrice.Value > 0
            ? phone.DiscountPrice.Value
            : phone.Price;

        var orderDetail = new OrderDetail
        {
          OrderId = order.Id,
          MobilePhoneId = od.MobilePhoneId,
          MobilePhoneColorId = od.MobilePhoneColorId, // có thể null

          Quantity = od.Quantity,
          UnitPrice = unitPrice
        };

        await _orderDetailRepository.InsertAsync(orderDetail);

        // Trừ tồn kho
        phone.StockQuantity -= od.Quantity;
        await _mobilePhoneRepository.UpdateAsync(phone);
      }

      // Load lại order cùng OrderDetails để tính tổng
      order = await _orderRepository.GetAll()
          .Include(o => o.OrderDetails)
          .FirstOrDefaultAsync(o => o.Id == order.Id);

      // Tính tổng tiền sản phẩm
      order.TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);

      // Tính phí ship
      order.ShippingFee = CalculateShippingFee(order.ShippingMethod);

      // ===================== TÍNH MÃ GIẢM GIÁ =====================
      if (!string.IsNullOrEmpty(input.DiscountCode) || input.DiscountId.HasValue)
      {
        Discount discount = null;

        if (input.DiscountId.HasValue)
        {
          discount = await _discountRepository.GetAsync(input.DiscountId.Value);
        }
        else
        {
          discount = await _discountRepository.GetAll()
              .FirstOrDefaultAsync(d => d.Code == input.DiscountCode);
        }

        if (discount == null)
          throw new UserFriendlyException("Mã giảm giá không tồn tại");

        // Chuyển OrderDetails sang OrderItemDto để tính discount
        var orderItems = input.OrderDetails.Select(od => new OrderItemDto
        {
          ProductId = od.MobilePhoneId,
          Quantity = od.Quantity,
          UnitPrice = od.UnitPrice
        }).ToList();

        // Kiểm tra điều kiện áp dụng
        await ValidateDiscountAsync(discount, order.TotalAmount, orderItems);

        // Tính số tiền giảm
        decimal discountAmount = await CalculateDiscountAmountAsync(discount, order.TotalAmount, orderItems);

        // Gán vào đơn
        order.DiscountId = discount.Id;
        order.DiscountAmount = discountAmount;

        // TĂNG SỐ LẦN SỬ DỤNG
        discount.CurrentUsage++;
        await _discountRepository.UpdateAsync(discount);
      }

      // Tổng cuối cùng
      order.FinalAmount = order.TotalAmount - order.DiscountAmount + order.ShippingFee;

      await _orderRepository.UpdateAsync(order);
      await CurrentUnitOfWork.SaveChangesAsync();

      return MapToOrderDto(order);
    }


    // ================== ÁP DỤNG MÃ GIẢM GIÁ ==================
    public async Task<ApplyDiscountResultDto> ApplyDiscountAsync(ApplyDiscountInputDto input)
    {
      if (string.IsNullOrEmpty(input.DiscountCode))
        throw new UserFriendlyException("Chưa nhập mã giảm giá");

      var discount = await _discountRepository.GetAll()
          .FirstOrDefaultAsync(d => d.Code == input.DiscountCode);

      if (discount == null)
        throw new UserFriendlyException("Mã giảm giá không tồn tại");

      // Kiểm tra điều kiện áp dụng
      await ValidateDiscountAsync(discount, input.TotalAmount, input.OrderItems);

      // Tính số tiền giảm
      decimal discountAmount = await CalculateDiscountAmountAsync(discount, input.TotalAmount, input.OrderItems);

      return new ApplyDiscountResultDto
      {
        DiscountId = discount.Id,
        DiscountCode = discount.Code,
        DiscountAmount = discountAmount,
        DiscountPercentage = discount.Percentage,
        EligibleAmount = await CalculateEligibleAmountAsync(discount, input.OrderItems)
      };
    }

    // ================== KIỂM TRA MÃ GIẢM GIÁ ==================
    private async Task ValidateDiscountAsync(Discount discount, decimal totalAmount, List<OrderItemDto> orderItems)
    {
      // Kiểm tra trạng thái mã
      if (!discount.IsActive)
        throw new UserFriendlyException("Mã giảm giá không khả dụng");

      // Kiểm tra ngày hiệu lực
      var now = DateTime.Now;
      if (now < discount.StartDate)
        throw new UserFriendlyException("Mã giảm giá chưa có hiệu lực");

      if (now > discount.EndDate)
        throw new UserFriendlyException("Mã giảm giá đã hết hạn");

      // Kiểm tra số lần sử dụng
      if (discount.MaxUsage > 0 && discount.CurrentUsage >= discount.MaxUsage)
        throw new UserFriendlyException("Mã giảm giá đã hết lượt sử dụng");

      // Kiểm tra giá trị đơn hàng tối thiểu
      if (totalAmount < discount.MinOrderValue)
        throw new UserFriendlyException($"Đơn hàng tối thiểu {discount.MinOrderValue.ToString("N0")} đ để áp dụng mã");

      // Kiểm tra có sản phẩm nào đủ điều kiện không
      if (discount.ApplyType != 0) // Không phải áp dụng toàn bộ đơn hàng
      {
        var eligibleAmount = await CalculateEligibleAmountAsync(discount, orderItems);
        if (eligibleAmount <= 0)
          throw new UserFriendlyException("Không có sản phẩm nào trong đơn hàng đủ điều kiện áp dụng mã giảm giá");
      }
    }

    // ================== TÍNH SỐ TIỀN ĐỦ ĐIỀU KIỆN ==================
    private async Task<decimal> CalculateEligibleAmountAsync(Discount discount, List<OrderItemDto> orderItems)
    {
      if (!orderItems.Any()) return 0m;

      decimal eligibleAmount = 0m;

      if (discount.ApplyType == 1) // Áp dụng cho danh mục
      {
        // Lấy danh sách category IDs từ bảng trung gian DiscountCategory
        var discountCategories = await _discountCategoryRepository.GetAll()
            .Where(dc => dc.DiscountId == discount.Id)
            .Select(dc => dc.CategoryId)
            .ToListAsync();

        if (discountCategories.Any())
        {
          var productIds = orderItems.Select(oi => oi.ProductId).ToList();

          var products = await _mobilePhoneRepository.GetAll()
              .Where(p => productIds.Contains(p.Id))
              .Select(p => new { p.Id, p.CategoryId })
              .ToListAsync();

          foreach (var orderItem in orderItems)
          {
            var product = products.FirstOrDefault(p => p.Id == orderItem.ProductId);
            if (product != null && discountCategories.Contains(product.CategoryId))
            {
              eligibleAmount += orderItem.UnitPrice * orderItem.Quantity;
            }
          }
        }
      }
      else if (discount.ApplyType == 2) // Áp dụng cho sản phẩm cụ thể
      {
        // Lấy danh sách product IDs từ bảng trung gian DiscountProduct
        var discountProducts = await _discountProductRepository.GetAll()
            .Where(dp => dp.DiscountId == discount.Id)
            .Select(dp => dp.MobilePhoneId)
            .ToListAsync();

        if (discountProducts.Any())
        {
          foreach (var orderItem in orderItems)
          {
            if (discountProducts.Contains(orderItem.ProductId))
            {
              eligibleAmount += orderItem.UnitPrice * orderItem.Quantity;
            }
          }
        }
      }
      else // Áp dụng cho toàn bộ đơn hàng
      {
        eligibleAmount = orderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
      }

      return eligibleAmount;
    }

    // ================== TÍNH SỐ TIỀN GIẢM GIÁ ==================
    private async Task<decimal> CalculateDiscountAmountAsync(Discount discount, decimal totalAmount, List<OrderItemDto> orderItems)
    {
      decimal eligibleAmount = await CalculateEligibleAmountAsync(discount, orderItems);

      // Nếu là áp dụng toàn bộ đơn hàng, dùng totalAmount
      if (discount.ApplyType == 0)
      {
        eligibleAmount = totalAmount;
      }

      decimal discountAmount = 0m;

      if (discount.Percentage.HasValue && discount.Percentage.Value > 0)
      {
        discountAmount = eligibleAmount * discount.Percentage.Value / 100;
      }
      else if (discount.Amount.HasValue && discount.Amount.Value > 0)
      {
        discountAmount = discount.Amount.Value;
        // Đảm bảo không giảm quá số tiền eligible
        if (discountAmount > eligibleAmount)
          discountAmount = eligibleAmount;
      }

      return discountAmount;
    }

    // ================== LẤY ĐƠN HÀNG THEO ID ==================
    public async Task<OrderDto> GetAsync(EntityDto<int> input)
    {
      var order = await _orderRepository.GetAll()
          .Include(o => o.OrderDetails)
              .ThenInclude(od => od.MobilePhone)
          .Include(o => o.OrderDetails)
              .ThenInclude(od => od.MobilePhoneColor) // 👈 Thêm dòng này để lấy màu (nếu có)
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

      // Nếu đang hủy đơn (status = 3) và chưa hủy trước đó, trả lại tồn kho
      if (input.Status == 3 && order.Status != 3)
      {
        foreach (var od in order.OrderDetails)
        {
          var phone = await _mobilePhoneRepository.GetAsync(od.MobilePhoneId);
          phone.StockQuantity += od.Quantity;
          await _mobilePhoneRepository.UpdateAsync(phone);
        }
      }

      // Cập nhật thông tin cơ bản
      order.RecipientName = input.RecipientName;
      order.RecipientAddress = input.RecipientAddress;
      order.RecipientPhone = input.RecipientPhone;
      order.PaymentMethod = input.PaymentMethod;
      order.ShippingMethod = input.ShippingMethod;
      order.Status = input.Status;
      order.Note = input.Note;

      // Cập nhật phí ship
      order.ShippingFee = CalculateShippingFee(order.ShippingMethod);

      // ===================== XỬ LÝ MÃ GIẢM GIÁ =====================
      if (!string.IsNullOrEmpty(input.DiscountCode) || input.DiscountId.HasValue)
      {
        Discount discount = null;

        if (input.DiscountId.HasValue)
        {
          discount = await _discountRepository.GetAsync(input.DiscountId.Value);
        }
        else
        {
          discount = await _discountRepository.GetAll()
              .FirstOrDefaultAsync(d => d.Code == input.DiscountCode);
        }

        if (discount != null)
        {
          var orderItems = order.OrderDetails.Select(od => new OrderItemDto
          {
            ProductId = od.MobilePhoneId,
            Quantity = od.Quantity,
            UnitPrice = od.UnitPrice
          }).ToList();

          await ValidateDiscountAsync(discount, order.TotalAmount, orderItems);

          var discountAmount = await CalculateDiscountAmountAsync(discount, order.TotalAmount, orderItems);

          order.DiscountId = discount.Id;
          order.DiscountAmount = discountAmount;
        }
        else
        {
          throw new UserFriendlyException("Mã giảm giá không tồn tại");
        }
      }
      else
      {
        // Nếu xóa voucher
        order.DiscountId = null;
        order.DiscountAmount = 0m;
      }

      // Tính tổng cuối cùng
      order.FinalAmount = order.TotalAmount - order.DiscountAmount + order.ShippingFee;

      await _orderRepository.UpdateAsync(order);
      await CurrentUnitOfWork.SaveChangesAsync();

      return MapToOrderDto(order);
    }

    // ================== PHÂN TRANG ==================
    public async Task<PagedResultDto<OrderDto>> GetAllAsync(PagedOrderResultRequestDto input)
    {
      var query = _orderRepository.GetAll()
          .Include(o => o.OrderDetails)
              .ThenInclude(od => od.MobilePhone)
          .Include(o => o.OrderDetails)
              .ThenInclude(od => od.MobilePhoneColor) // 👈 Thêm include màu
          .Include(o => o.Discount)
          .AsQueryable();

      // ⭐ Lọc theo từ khóa
      if (!string.IsNullOrWhiteSpace(input.Keyword))
      {
        query = query.Where(o =>
            o.RecipientName.Contains(input.Keyword) ||
            o.RecipientPhone.Contains(input.Keyword));
      }

      // Lọc theo User
      if (input.UserId.HasValue)
        query = query.Where(o => o.UserId == input.UserId.Value);

      // Lọc theo trạng thái
      if (input.Status.HasValue && input.Status.Value >= 0)
        query = query.Where(o => o.Status == input.Status.Value);

      // Đếm tổng
      var totalCount = await query.CountAsync();

      // Lấy dữ liệu trang
      var items = await query
          .OrderByDescending(o => o.CreationTime)
          .Skip(input.SkipCount)
          .Take(input.MaxResultCount)
          .ToListAsync();

      // Map sang DTO
      return new PagedResultDto<OrderDto>(
          totalCount,
          items.Select(MapToOrderDto).ToList()
      );
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
        // 🟢 Màu sắc (có thể null)
        MobilePhoneColorId = od.MobilePhoneColorId,
        ColorName = od.MobilePhoneColor?.ColorName,

        // Ưu tiên ảnh theo màu nếu có, nếu không thì lấy ảnh mặc định
        ImageUrl = od.MobilePhoneColor?.ImageUrl ?? od.MobilePhone?.ImageUrl,
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
    // ================== LẤY DANH SÁCH MÃ GIẢM GIÁ KHẢ DỤNG ==================
    // ================== LẤY DANH SÁCH MÃ GIẢM GIÁ KHẢ DỤNG ==================
    public async Task<List<DiscountDto>> GetAvailableDiscountsAsync(List<OrderItemDto> cartItems, decimal cartTotalAmount)
    {
      var now = DateTime.Now;

      // Lấy danh sách discount khả dụng
      var discounts = await _discountRepository.GetAll()
          .Where(d => d.IsActive
              && d.StartDate <= now
              && d.EndDate >= now
              && (d.MaxUsage == 0 || d.CurrentUsage < d.MaxUsage))
          .ToListAsync();

      var discountDtos = new List<DiscountDto>();

      foreach (var discount in discounts)
      {
        var discountCategories = new List<int>();
        var discountProducts = new List<int>();

        if (discount.ApplyType == 1) // Danh mục
        {
          discountCategories = await _discountCategoryRepository.GetAll()
              .Where(dc => dc.DiscountId == discount.Id)
              .Select(dc => dc.CategoryId)
              .ToListAsync();
        }
        else if (discount.ApplyType == 2) // Sản phẩm cụ thể
        {
          discountProducts = await _discountProductRepository.GetAll()
              .Where(dp => dp.DiscountId == discount.Id)
              .Select(dp => dp.MobilePhoneId)
              .ToListAsync();
        }

        // Kiểm tra điều kiện áp dụng
        bool canApply = false;

        if (discount.ApplyType == 0)
        {
          canApply = cartTotalAmount >= discount.MinOrderValue;
        }
        else
        {
          decimal eligibleAmount = await CalculateEligibleAmountAsync(discount, cartItems);
          canApply = eligibleAmount > 0 && cartTotalAmount >= discount.MinOrderValue;
        }

        if (!canApply) continue;

        var discountDto = new DiscountDto
        {
          Id = discount.Id,
          Name = discount.Name,
          Code = discount.Code,
          Percentage = discount.Percentage,
          Amount = discount.Amount,
          MinOrderValue = discount.MinOrderValue,
          ApplyType = discount.ApplyType,
          MaxUsage = discount.MaxUsage,
          CurrentUsage = discount.CurrentUsage,
          StartDate = discount.StartDate,
          EndDate = discount.EndDate,
          IsActive = discount.IsActive,
          CreationTime = discount.CreationTime,
          ApplyScopeInfo = await GetApplyScopeInfoAsync(discount.ApplyType, discountCategories, discountProducts)
        };

        discountDtos.Add(discountDto);
      }

      return discountDtos;
    }

    // ================== LẤY THÔNG TIN PHẠM VI ÁP DỤNG ==================
    private async Task<string> GetApplyScopeInfoAsync(int applyType, List<int> categoryIds, List<int> productIds)
    {
      return applyType switch
      {
        0 => "Áp dụng cho toàn bộ đơn hàng",
        1 => await GetCategoryNamesAsync(categoryIds),
        2 => await GetProductNamesAsync(productIds),
        _ => "Không xác định"
      };
    }

    // ================== LẤY TÊN DANH MỤC ==================
    private async Task<string> GetCategoryNamesAsync(List<int> categoryIds)
    {
      if (!categoryIds.Any())
        return "Áp dụng cho danh mục (chưa cấu hình)";

      return $"Áp dụng cho {categoryIds.Count} danh mục";
    }

    // ================== LẤY TÊN SẢN PHẨM ==================
    private async Task<string> GetProductNamesAsync(List<int> productIds)
    {
      if (!productIds.Any())
        return "Áp dụng cho sản phẩm (chưa cấu hình)";

      var products = await _mobilePhoneRepository.GetAll()
          .Where(p => productIds.Contains(p.Id))
          .Select(p => p.Name)
          .Take(2) // Chỉ lấy 2 sản phẩm đầu để hiển thị
          .ToListAsync();

      var productNames = string.Join(", ", products);
      if (productIds.Count > 2)
      {
        productNames += $", ... và {productIds.Count - 2} sản phẩm khác";
      }

      return $"Áp dụng cho: {productNames}";
    }
    public async Task UpdateStatusAsync(int orderId, int status)
    {
      var order = await _orderRepository.GetAll()
          .Include(o => o.OrderDetails)
          .FirstOrDefaultAsync(o => o.Id == orderId);

      if (order != null)
      {
        // Trả lại tồn kho nếu hủy đơn và chưa hủy trước đó
        if (status == 3 && order.Status != 3)
        {
          foreach (var od in order.OrderDetails)
          {
            var phone = await _mobilePhoneRepository.GetAsync(od.MobilePhoneId);
            phone.StockQuantity += od.Quantity;
            await _mobilePhoneRepository.UpdateAsync(phone);
          }
        }

        order.Status = status;
        await _orderRepository.UpdateAsync(order);
        await CurrentUnitOfWork.SaveChangesAsync();
      }
    }
  }
}