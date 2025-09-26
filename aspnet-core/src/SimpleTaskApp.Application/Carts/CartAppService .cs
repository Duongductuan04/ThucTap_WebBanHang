using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.MobilePhones.Dto;
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
        private readonly IAbpSession _abpSession;

        public CartAppService(
            IRepository<Cart, int> cartRepository,
            IRepository<MobilePhone, int> mobilePhoneRepository,
            IAbpSession abpSession)
        {
            _cartRepository = cartRepository;
            _mobilePhoneRepository = mobilePhoneRepository;
            _abpSession = abpSession;
        }

        // CREATE
        [AbpAuthorize]
        public async Task<CartDto> AddToCartAsync(CreateCartDto input)
        {
            var phone = await _mobilePhoneRepository.GetAsync(input.MobilePhoneId);

            var existing = await _cartRepository.FirstOrDefaultAsync(
                c => c.UserId == AbpSession.UserId && c.MobilePhoneId == input.MobilePhoneId
            );

            if (existing != null)
            {
                existing.Quantity += input.Quantity;
                await _cartRepository.UpdateAsync(existing);
                return MapToDto(existing, phone);
            }

            var cart = new Cart
            {
                UserId = AbpSession.UserId.Value,
                MobilePhoneId = input.MobilePhoneId,
                Quantity = input.Quantity
            };

            await _cartRepository.InsertAsync(cart);
            return MapToDto(cart, phone);
        }

        // GET BY ID
        public async Task<CartDto> GetAsync(EntityDto<int> input)
        {
            var cart = await _cartRepository.GetAllIncluding(c => c.MobilePhone)
                                            .FirstOrDefaultAsync(c => c.Id == input.Id);

            if (cart == null)
                throw new UserFriendlyException($"Không tìm thấy giỏ hàng! Id = {input.Id}");

            return MapToDto(cart, cart.MobilePhone);
        }

        // UPDATE
        [AbpAuthorize]
        public async Task<CartDto> UpdateAsync(UpdateCartDto input)
        {
            var cart = await _cartRepository.GetAllIncluding(c => c.MobilePhone)
                                            .FirstOrDefaultAsync(c => c.Id == input.Id);

            if (cart == null)
                throw new UserFriendlyException($"Không tìm thấy giỏ hàng! Id = {input.Id}");

            cart.Quantity = input.Quantity;
            await _cartRepository.UpdateAsync(cart);

            return MapToDto(cart, cart.MobilePhone);
        }

        // UPDATE QUANTITY (AJAX)
        [AbpAuthorize]
        public async Task UpdateQuantityAsync(int cartId, int quantity)
        {
            var cart = await _cartRepository.FirstOrDefaultAsync(cartId);

            if (cart == null)
                throw new UserFriendlyException("Không tìm thấy sản phẩm trong giỏ hàng!");

            if (cart.UserId != AbpSession.UserId)
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

        // GET ALL
        public async Task<PagedResultDto<CartDto>> GetAllAsync(PagedCartResultRequestDto input)
        {
            var query = _cartRepository.GetAllIncluding(c => c.MobilePhone);

            if (input.UserId.HasValue)
            {
                query = query.Where(c => c.UserId == input.UserId.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(c => c.Id)
                                   .Skip(input.SkipCount)
                                   .Take(input.MaxResultCount)
                                   .ToListAsync();

            var result = items.Select(c => MapToDto(c, c.MobilePhone)).ToList();
            return new PagedResultDto<CartDto>(totalCount, result);
        }

        // GET CART TOTAL
        public async Task<decimal> GetCartTotalAsync(long userId)
        {
            var items = await _cartRepository.GetAllIncluding(c => c.MobilePhone)
                                             .Where(c => c.UserId == userId)
                                             .ToListAsync();

            return items.Sum(c => (c.MobilePhone.DiscountPrice ?? c.MobilePhone.Price) * c.Quantity);
        }

        // GET MY CART
        public async Task<List<CartDto>> GetMyCartAsync()
        {
            var userId = _abpSession.UserId.Value;

            var carts = await _cartRepository
                .GetAll()
                .Include(x => x.MobilePhone)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return carts.Select(c => MapToDto(c, c.MobilePhone)).ToList();
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

        // HELPER
        private CartDto MapToDto(Cart cart, MobilePhone phone)
        {
            if (cart == null) return null;

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                MobilePhoneId = cart.MobilePhoneId,
                Name = phone?.Name,
                ImageUrl = phone?.ImageUrl,
                Price = phone?.Price ?? 0,
                DiscountPrice = phone?.DiscountPrice,
                Quantity = cart.Quantity,
                StockQuantity = phone?.StockQuantity ?? 0
            };
        }
    }
}