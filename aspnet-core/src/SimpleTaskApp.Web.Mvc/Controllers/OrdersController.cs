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
    private readonly IDiscountAppService _discountAppService;

    public OrdersController(
        IOrderAppService orderAppService,
        ICartAppService cartAppService,
            IDiscountAppService discountAppService,// thêm vào

        IMobilePhoneAppService mobilePhoneAppService)
    {
        _orderAppService = orderAppService;
        _cartAppService = cartAppService;
        _discountAppService = discountAppService; // gán

        _mobilePhoneAppService = mobilePhoneAppService;
    }

    // ==========================
    // 1. HIỂN THỊ CHECKOUT MUA NGAY
    // ==========================
    [HttpGet]
    public async Task<IActionResult> CheckoutBuyNow(int mobilePhoneId, int quantity)
    {
        var mobilePhone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(mobilePhoneId));
        if (mobilePhone == null)
            return RedirectToAction("Index", "Home");

        var orderDto = new CreateOrderDto
        {
            OrderDetails = new List<CreateOrderDetailDto>
            {
                new CreateOrderDetailDto
                {
                    MobilePhoneId = mobilePhone.Id,
                    Quantity = quantity,
                    UnitPrice = mobilePhone.DiscountPrice ?? mobilePhone.Price,
                    MobilePhoneName = mobilePhone.Name,
                    ImageUrl = mobilePhone.ImageUrl
                }
            }
        };

        // Tạm tính tổng tiền để hiển thị
        orderDto.TotalAmount = orderDto.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
        orderDto.ShippingFee = 20000; // Mặc định phí ship tiêu chuẩn
        orderDto.DiscountAmount = 0m; // Chưa áp dụng mã giảm giá lúc hiển thị
        orderDto.FinalAmount = orderDto.TotalAmount - (orderDto.DiscountAmount ?? 0m) + orderDto.ShippingFee;
        // Lấy danh sách voucher áp dụng đúng giỏ hàng
        var availableDiscounts = await _orderAppService.GetAvailableDiscountsAsync(
            orderDto.OrderDetails.Select(od => new OrderItemDto
            {
                ProductId = od.MobilePhoneId,
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice
            }).ToList(),
            orderDto.TotalAmount
        );
        ViewBag.AvailableDiscounts = availableDiscounts;
        return View("CheckoutBuyNow", orderDto);
    }

    // ==========================
    // 2. HIỂN THỊ CHECKOUT GIỎ HÀNG
    // ==========================
    [HttpGet]
    public async Task<IActionResult> CheckoutCart(List<int> cartIds)
    {
        var myCart = await _cartAppService.GetMyCartAsync() ?? new List<CartDto>();
        var selectedItems = myCart.Where(c => cartIds.Contains(c.Id)).ToList();

        if (!selectedItems.Any())
            return RedirectToAction("Index", "Cart");

        var orderDto = new CreateOrderDto
        {
            OrderDetails = selectedItems.Select(c => new CreateOrderDetailDto
            {
                MobilePhoneId = c.MobilePhoneId,
                Quantity = c.Quantity,
                UnitPrice = c.DisplayPrice,
                MobilePhoneName = c.Name,
                ImageUrl = c.ImageUrl
            }).ToList()
        };

        // Tạm tính tổng tiền để hiển thị
        orderDto.TotalAmount = orderDto.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
        orderDto.ShippingFee = 20000; // Mặc định phí ship tiêu chuẩn
        orderDto.DiscountAmount = 0m; // Chưa áp dụng mã giảm giá lúc hiển thị
        orderDto.FinalAmount = orderDto.TotalAmount - (orderDto.DiscountAmount ?? 0m) + orderDto.ShippingFee;
        // Lấy danh sách voucher áp dụng đúng giỏ hàng
        var availableDiscounts = await _orderAppService.GetAvailableDiscountsAsync(
            orderDto.OrderDetails.Select(od => new OrderItemDto
            {
                ProductId = od.MobilePhoneId,
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice
            }).ToList(),
            orderDto.TotalAmount
        );
        ViewBag.AvailableDiscounts = availableDiscounts;
        return View("CheckoutCart", orderDto);
    }

    // ==========================
    // 3. XỬ LÝ MUA NGAY
    // ==========================
    [HttpPost]
    public async Task<IActionResult> BuyNow(CreateOrderDto input,
        [FromForm] int MobilePhoneId,
        [FromForm] int Quantity,
        [FromForm] string DiscountCode)  // <-- thêm mã giảm giá
    {
        var mobilePhone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(MobilePhoneId));
        if (mobilePhone == null)
        {
            ModelState.AddModelError("", "Sản phẩm không tồn tại.");
            return RedirectToAction("CheckoutBuyNow", new { mobilePhoneId = MobilePhoneId, quantity = Quantity });
        }

        input.OrderDetails = new List<CreateOrderDetailDto>
        {
            new CreateOrderDetailDto
            {
                MobilePhoneId = mobilePhone.Id,
                Quantity = Quantity,
                UnitPrice = mobilePhone.DiscountPrice ?? mobilePhone.Price,
                MobilePhoneName = mobilePhone.Name,
                ImageUrl = mobilePhone.ImageUrl
            }
        };

        input.DiscountCode = DiscountCode; // <-- gán mã giảm giá

        await _orderAppService.CreateAsync(input); // Voucher xử lý ở service

        return RedirectToAction("Success");
    }

    // ==========================
    // 4. XỬ LÝ THANH TOÁN GIỎ HÀNG
    // ==========================
    [HttpPost]
    public async Task<IActionResult> CheckoutCart(CreateOrderDto input,
        [FromForm] List<int> cartIds,
        [FromForm] string DiscountCode) // <-- thêm mã giảm giá
    {
        cartIds ??= new List<int>();
        var myCart = await _cartAppService.GetMyCartAsync() ?? new List<CartDto>();
        if (cartIds.Any())
            myCart = myCart.Where(c => cartIds.Contains(c.Id)).ToList();

        input.OrderDetails = myCart
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
            return View("CheckoutCart", input);
        }

        input.DiscountCode = DiscountCode; // <-- gán mã giảm giá

        await _orderAppService.CreateAsync(input); // Voucher xử lý ở service

        // Xóa sản phẩm khỏi giỏ hàng sau khi đặt
        foreach (var cartId in cartIds)
            await _cartAppService.DeleteAsync(new EntityDto<int>(cartId));

        return RedirectToAction("Success");
    }

    // ==========================
    // 5. TRANG THÀNH CÔNG
    // ==========================
    public IActionResult Success()
    {
        return View();
    }
}
