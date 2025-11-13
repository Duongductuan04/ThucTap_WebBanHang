using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.MobilePhones.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
  [AbpAuthorize]
  public class CartAppService : SimpleTaskAppAppServiceBase, ICartAppService
  {
    private readonly IRepository<Cart, int> _cartRepository;
    private readonly IRepository<MobilePhone, int> _mobilePhoneRepository;
    private readonly IRepository<MobilePhoneColor, int> _mobilePhoneColorRepository; // ✅ Thêm repository màu
    private readonly IAbpSession _abpSession;

    public CartAppService(
        IRepository<Cart, int> cartRepository,
        IRepository<MobilePhone, int> mobilePhoneRepository,
        IRepository<MobilePhoneColor, int> mobilePhoneColorRepository,
        IAbpSession abpSession)
    {
      _cartRepository = cartRepository;
      _mobilePhoneRepository = mobilePhoneRepository;
      _mobilePhoneColorRepository = mobilePhoneColorRepository;
      _abpSession = abpSession;
    }

    // CREATE / ADD TO CART
    [AbpAuthorize]
    public async Task<CartDto> AddToCartAsync(CreateCartDto input)
    {
      var phone = await _mobilePhoneRepository.GetAsync(input.MobilePhoneId);

      MobilePhoneColor color = null;
      if (input.MobilePhoneColorId.HasValue)
      {
        color = await _mobilePhoneColorRepository.FirstOrDefaultAsync(input.MobilePhoneColorId.Value);
        if (color == null)
          throw new UserFriendlyException("Màu bạn chọn không tồn tại!");
      }

      var existing = await _cartRepository.FirstOrDefaultAsync(
    c => c.UserId == _abpSession.UserId
         && c.MobilePhoneId == input.MobilePhoneId
         && c.MobilePhoneColorId == (int?)input.MobilePhoneColorId
);
      if (existing != null)
      {
        existing.Quantity += input.Quantity;
        await _cartRepository.UpdateAsync(existing);
        return MapToDto(existing, phone, color);
      }

      var cart = new Cart
      {
        UserId = _abpSession.UserId.Value,
        MobilePhoneId = input.MobilePhoneId,
        MobilePhoneColorId = input.MobilePhoneColorId,
        Quantity = input.Quantity
      };

      await _cartRepository.InsertAsync(cart);
      return MapToDto(cart, phone, color);
    }

    // GET BY ID
    public async Task<CartDto> GetAsync(EntityDto<int> input)
    {
      var cart = await _cartRepository.GetAllIncluding(c => c.MobilePhone, c => c.MobilePhoneColor)
                                      .FirstOrDefaultAsync(c => c.Id == input.Id);

      if (cart == null)
        throw new UserFriendlyException($"Không tìm thấy giỏ hàng! Id = {input.Id}");

      return MapToDto(cart, cart.MobilePhone, cart.MobilePhoneColor);
    }

    // UPDATE CART
    [AbpAuthorize]
    public async Task<CartDto> UpdateAsync(UpdateCartDto input)
    {
      var cart = await _cartRepository.GetAllIncluding(c => c.MobilePhone, c => c.MobilePhoneColor)
                                      .FirstOrDefaultAsync(c => c.Id == input.Id);

      if (cart == null)
        throw new UserFriendlyException($"Không tìm thấy giỏ hàng! Id = {input.Id}");

      cart.Quantity = input.Quantity;
      await _cartRepository.UpdateAsync(cart);

      return MapToDto(cart, cart.MobilePhone, cart.MobilePhoneColor);
    }

    // UPDATE QUANTITY (AJAX)
    [AbpAuthorize]
    public async Task UpdateQuantityAsync(int cartId, int quantity)
    {
      var cart = await _cartRepository.FirstOrDefaultAsync(cartId);

      if (cart == null)
        throw new UserFriendlyException("Không tìm thấy sản phẩm trong giỏ hàng!");

      if (cart.UserId != _abpSession.UserId)
        throw new AbpAuthorizationException("Bạn không có quyền chỉnh sửa sản phẩm này!");

      if (quantity <= 0)
        throw new UserFriendlyException("Số lượng sản phẩm phải lớn hơn 0!");

      cart.Quantity = quantity;
      await _cartRepository.UpdateAsync(cart);
    }

    // DELETE
    public async Task DeleteAsync(EntityDto<int> input)
    {
      await _cartRepository.DeleteAsync(input.Id);
    }

    // GET ALL (PAGED)
    public async Task<PagedResultDto<CartDto>> GetAllAsync(PagedCartResultRequestDto input)
    {
      var query = _cartRepository.GetAllIncluding(c => c.MobilePhone, c => c.MobilePhoneColor);

      if (input.UserId.HasValue)
      {
        query = query.Where(c => c.UserId == input.UserId.Value);
      }

      var totalCount = await query.CountAsync();
      var items = await query.OrderBy(c => c.Id)
                             .Skip(input.SkipCount)
                             .Take(input.MaxResultCount)
                             .ToListAsync();

      var result = items.Select(c => MapToDto(c, c.MobilePhone, c.MobilePhoneColor)).ToList();
      return new PagedResultDto<CartDto>(totalCount, result);
    }

    // GET CART TOTAL
    public async Task<decimal> GetCartTotalAsync(long userId)
    {
      var items = await _cartRepository.GetAllIncluding(c => c.MobilePhone)
                                       .Where(c => c.UserId == userId)
                                       .ToListAsync();

      return items.Sum(c => GetEffectivePrice(c.MobilePhone) * c.Quantity);
    }

    // GET MY CART
    public async Task<List<CartDto>> GetMyCartAsync()
    {
      var userId = _abpSession.UserId.Value;

      var carts = await _cartRepository
          .GetAll()
          .Include(x => x.MobilePhone)
          .Include(x => x.MobilePhoneColor)
          .Where(x => x.UserId == userId)
          .ToListAsync();

      return carts.Select(c => MapToDto(c, c.MobilePhone, c.MobilePhoneColor)).ToList();
    }

    // CLEAR MY CART
    public async Task ClearMyCartAsync()
    {
      var userId = _abpSession.UserId.Value;

      var carts = await _cartRepository
          .GetAll()
          .Where(x => x.UserId == userId)
          .ToListAsync();

      foreach (var cart in carts)
      {
        await _cartRepository.DeleteAsync(cart);
      }
    }

    // HELPER: Map Cart -> CartDto
    private CartDto MapToDto(Cart cart, MobilePhone phone, MobilePhoneColor color = null)
    {
      if (cart == null) return null;

      return new CartDto
      {
        Id = cart.Id,
        UserId = cart.UserId,
        MobilePhoneId = cart.MobilePhoneId,
        Name = phone?.Name ?? "Sản phẩm đã bị xóa",
        ImageUrl = phone?.ImageUrl,
        Price = phone?.Price ?? 0,
        DiscountPrice = phone?.DiscountPrice,
        DisplayPrice = GetEffectivePrice(phone),
        Quantity = cart.Quantity,
        StockQuantity = phone?.StockQuantity ?? 0,
        MobilePhoneColorId = cart.MobilePhoneColorId, // ⚡ Thêm dòng này

        ColorName = color?.ColorName ?? cart.MobilePhoneColor?.ColorName,
        ColorImageUrl = color?.ImageUrl ?? cart.MobilePhoneColor?.ImageUrl,
        IsAvailable = phone != null,
        IsOutOfStock = phone != null && phone.StockQuantity <= 0,
        StatusMessage = phone == null ? "Sản phẩm không tồn tại hoặc đã bị xóa" :
                          phone.StockQuantity <= 0 ? "Đã hết hàng" : "Còn hàng"
      };
    }

    // HELPER: Tính giá thực tế
    private decimal GetEffectivePrice(MobilePhone phone)
    {
      if (phone == null) return 0;

      bool isOnSale = phone.IsOnSale;

      if (phone.SaleStart.HasValue && phone.SaleEnd <= DateTime.Now)
      {
        isOnSale = false;
      }

      if (isOnSale && phone.DiscountPrice.HasValue && phone.DiscountPrice.Value < phone.Price)
      {
        return phone.DiscountPrice.Value;
      }

      return phone.Price;
    }
  }
}
