using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OrdersController : AbpController
{
    private readonly IOrderAppService _orderAppService;
    private readonly ICartAppService _cartAppService;
    private readonly IMobilePhoneAppService _mobilePhoneAppService;


    public OrdersController(IOrderAppService orderAppService, ICartAppService cartAppService, IMobilePhoneAppService mobilePhoneAppService)
    {
        _orderAppService = orderAppService;
        _cartAppService = cartAppService;
        _mobilePhoneAppService = mobilePhoneAppService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Checkout");
    }
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto input,
     [FromForm] List<int> cartIds,
     [FromForm] int? MobilePhoneId,
     [FromForm] int? Quantity)
    {
        // Khởi tạo cartIds nếu null
        cartIds ??= new List<int>();

        List<CartDto> cartItems = new List<CartDto>();

        if (MobilePhoneId.HasValue && Quantity.HasValue)
        {
            // Mua Ngay
            var mobilePhone = await _mobilePhoneAppService.GetAsync(new EntityDto<int> { Id = MobilePhoneId.Value });
            if (mobilePhone != null)
            {
                cartItems.Add(new CartDto
                {
                    Id = 0,
                    MobilePhoneId = mobilePhone.Id,
                    Quantity = Quantity.Value,
                    Name = mobilePhone.Name,
                    ImageUrl = mobilePhone.ImageUrl,
                    Price = mobilePhone.Price,
                    DiscountPrice = mobilePhone.DiscountPrice
                });
            }
        }
        else
        {
            // Mua từ giỏ hàng
            var myCart = await _cartAppService.GetMyCartAsync() ?? new List<CartDto>();
            if (cartIds.Any())
            {
                myCart = myCart.Where(c => cartIds.Contains(c.Id)).ToList();
            }
            cartItems = myCart;
        }

        // Map sang OrderDetails
        input.OrderDetails = cartItems
            .Where(x => x.Quantity > 0)
            .Select(x => new CreateOrderDetailDto
            {
                MobilePhoneId = x.MobilePhoneId,
                Quantity = x.Quantity,
                UnitPrice = x.DisplayPrice,
                MobilePhoneName = x.Name,
                ImageUrl = x.ImageUrl
            }).ToList();

        if (!input.OrderDetails.Any())
        {
            ModelState.AddModelError("", "Không có sản phẩm hợp lệ để tạo đơn hàng.");
            return View("Checkout", input);
        }

        // Tính tổng tiền
        input.TotalAmount = input.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);

        await _orderAppService.CreateAsync(input);

        // Xóa khỏi giỏ hàng nếu là giỏ
        if (!MobilePhoneId.HasValue && cartIds.Any())
        {
            foreach (var cartId in cartIds)
            {
                await _cartAppService.DeleteAsync(new EntityDto<int>(cartId));
            }
        }

        return RedirectToAction("Success");
    }

    [HttpGet]
    public async Task<IActionResult> Checkout([FromQuery] List<int> cartIds,
        [FromQuery] int? mobilePhoneId,
        [FromQuery] int? quantity)
    {
        List<CartDto> cartItems = new List<CartDto>();

        if (mobilePhoneId.HasValue && quantity.HasValue)
        {
            // Mua Ngay
            var mobilePhone = await _mobilePhoneAppService.GetAsync(new EntityDto<int> { Id = mobilePhoneId.Value });
            if (mobilePhone != null)
            {
                cartItems.Add(new CartDto
                {
                    Id = 0,
                    MobilePhoneId = mobilePhone.Id,
                    Quantity = quantity.Value,
                    Name = mobilePhone.Name,
                    ImageUrl = mobilePhone.ImageUrl,
                    Price = mobilePhone.Price,
                    DiscountPrice = mobilePhone.DiscountPrice
                });
            }
        }
        else
        {
            // Giỏ hàng
            var myCart = await _cartAppService.GetMyCartAsync() ?? new List<CartDto>();
            if (cartIds != null && cartIds.Any())
            {
                myCart = myCart.Where(c => cartIds.Contains(c.Id)).ToList();
            }
            cartItems = myCart;
        }

        var orderDto = new CreateOrderDto
        {
            OrderDetails = cartItems.Select(c => new CreateOrderDetailDto
            {
                MobilePhoneId = c.MobilePhoneId,
                Quantity = c.Quantity,
                UnitPrice = c.DisplayPrice,
                MobilePhoneName = c.Name,
                ImageUrl = c.ImageUrl
            }).ToList()
        };

        return View(orderDto);
    }

    public IActionResult Success()
    {
        return View();
    }
}