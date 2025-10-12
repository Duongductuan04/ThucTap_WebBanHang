using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.MobilePhones.Dto;
using SimpleTaskApp.Otp;
using SimpleTaskApp.Otp.Dto;
using SimpleTaskApp.Vnpay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OrdersController : AbpController
{
    private readonly IOrderAppService _orderAppService;
    private readonly ICartAppService _cartAppService;
    private readonly IMobilePhoneAppService _mobilePhoneAppService;
    private readonly IDiscountAppService _discountAppService;
    private readonly IVnPayService _vnPayService;
    private readonly IOtpAppService _otpService;

    public OrdersController(
        IOrderAppService orderAppService,
        ICartAppService cartAppService,
        IDiscountAppService discountAppService,
        IVnPayService vnPayService,
        IMobilePhoneAppService mobilePhoneAppService,
        IOtpAppService otpService
    )
    {
        _orderAppService = orderAppService;
        _cartAppService = cartAppService;
        _discountAppService = discountAppService;
        _vnPayService = vnPayService;
        _mobilePhoneAppService = mobilePhoneAppService;
        _otpService = otpService;
    }

    // ========================== OTP Actions ==========================
    [IgnoreAntiforgeryToken]
    [HttpPost]
    public async Task<JsonResult> SendOtp([FromBody] SendOtpDto input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.PhoneNumber))
        {
            return Json(new { success = false, message = "Số điện thoại không hợp lệ" });
        }

        try
        {
            var result = await _otpService.SendOtpAsync(input);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                otpCode = result.OtpCode // Chỉ dùng trong môi trường test/dev
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi khi gửi OTP: " + ex.Message });
        }
    }

    // ========================== Checkout mua ngay ==========================
    [HttpGet]
    public async Task<IActionResult> CheckoutBuyNow(int mobilePhoneId, int quantity)
    {
        var mobilePhone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(mobilePhoneId));
        if (mobilePhone == null) return RedirectToAction("Index", "Home");

        var unitPrice = GetEffectivePrice(mobilePhone);

        var orderDto = new CreateOrderDto
        {
            OrderDetails = new List<CreateOrderDetailDto>
            {
                new CreateOrderDetailDto
                {
                    MobilePhoneId = mobilePhone.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    MobilePhoneName = mobilePhone.Name,
                    ImageUrl = mobilePhone.ImageUrl
                }
            }
        };

        orderDto.TotalAmount = orderDto.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
        orderDto.ShippingFee = 20000;
        orderDto.DiscountAmount = 0m;
        orderDto.FinalAmount = orderDto.TotalAmount + orderDto.ShippingFee;

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

    // ========================== Checkout giỏ hàng ==========================
    [HttpGet]
    public async Task<IActionResult> CheckoutCart(List<int> cartIds)
    {
        var myCart = await _cartAppService.GetMyCartAsync() ?? new List<CartDto>();
        var selectedItems = myCart.Where(c => cartIds.Contains(c.Id)).ToList();
        if (!selectedItems.Any()) return RedirectToAction("Index", "Cart");

        // Lấy giá cập nhật từ dịch vụ MobilePhone cho mỗi item để preview chính xác
        var orderDetails = new List<CreateOrderDetailDto>();
        foreach (var c in selectedItems)
        {
            var phone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(c.MobilePhoneId));
            var unitPrice = GetEffectivePrice(phone);
            orderDetails.Add(new CreateOrderDetailDto
            {
                MobilePhoneId = c.MobilePhoneId,
                Quantity = c.Quantity,
                UnitPrice = unitPrice,
                MobilePhoneName = c.Name,
                ImageUrl = c.ImageUrl
            });
        }

        var orderDto = new CreateOrderDto
        {
            OrderDetails = orderDetails
        };

        orderDto.TotalAmount = orderDto.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
        orderDto.ShippingFee = 20000;
        orderDto.DiscountAmount = 0m;
        orderDto.FinalAmount = orderDto.TotalAmount + orderDto.ShippingFee;

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

    [HttpPost]
    public async Task<IActionResult> BuyNow(CreateOrderDto input,
      [FromForm] int MobilePhoneId,
      [FromForm] int Quantity,
      [FromForm] string DiscountCode,
      [FromForm] int PaymentMethod,
      [FromForm] string OtpCode)
    {
        // Tạo DTO VerifyOtpDto
        var verifyDto = new VerifyOtpDto
        {
            PhoneNumber = input.RecipientPhone,
            OtpCode = OtpCode
        };

        // Gọi service với DTO
        var otpResult = await _otpService.VerifyOtpAsync(verifyDto);

        if (!otpResult.Success)
        {
            TempData["Error"] = otpResult.Message;
            return RedirectToAction("CheckoutBuyNow", new { mobilePhoneId = MobilePhoneId, quantity = Quantity });
        }

        var mobilePhone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(MobilePhoneId));
        if (mobilePhone == null)
            return RedirectToAction("CheckoutBuyNow", new { mobilePhoneId = MobilePhoneId, quantity = Quantity });

        // Gán OrderDetails: sử dụng giá thực tính tại thời điểm thanh toán
        var unitPrice = GetEffectivePrice(mobilePhone);

        input.OrderDetails = new List<CreateOrderDetailDto>
        {
            new CreateOrderDetailDto
            {
                MobilePhoneId = mobilePhone.Id,
                Quantity = Quantity,
                UnitPrice = unitPrice,
                MobilePhoneName = mobilePhone.Name,
                ImageUrl = mobilePhone.ImageUrl
            }
        };

        input.DiscountCode = DiscountCode;
        input.PaymentMethod = PaymentMethod;

        // Lưu đơn hàng vào DB với trạng thái 0
        input.Status = 0; // chờ thanh toán
        var createdOrder = await _orderAppService.CreateAsync(input);

        // Nếu chọn COD, cập nhật trạng thái thành 1 (thành công)
        if (PaymentMethod != 1) // COD
        {
            await _orderAppService.UpdateStatusAsync(createdOrder.Id, 1);
        }
        if (PaymentMethod == 1) // VNPAY
        {
            // Tạo Payment URL
            var paymentInfo = new PaymentInformationModel
            {
                Amount = (long)createdOrder.FinalAmount,
                OrderDescription = "Thanh toán đơn hàng qua VNPAY",
                OrderType = "other",
                Name = input.RecipientName ?? User.Identity?.Name ?? "Khách hàng",
                OrderId = createdOrder.Id.ToString()
            };

            var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);
            return Redirect(paymentUrl);
        }

        return RedirectToAction("Success");
    }

    [HttpPost]
    public async Task<IActionResult> CheckoutCart(CreateOrderDto input,
     [FromForm] List<int> cartIds,
     [FromForm] string DiscountCode,
     [FromForm] int PaymentMethod,
     [FromForm] string OtpCode)
    {
        // Tạo DTO VerifyOtpDto
        var verifyDto = new VerifyOtpDto
        {
            PhoneNumber = input.RecipientPhone,
            OtpCode = OtpCode
        };

        // Kiểm tra OTP trước khi tạo đơn hàng
        var otpResult = await _otpService.VerifyOtpAsync(verifyDto);

        if (!otpResult.Success)
        {
            TempData["Error"] = otpResult.Message;
            return RedirectToAction("CheckoutCart", new { cartIds = cartIds });
        }

        cartIds ??= new List<int>();
        var myCart = await _cartAppService.GetMyCartAsync() ?? new List<CartDto>();
        if (cartIds.Any())
            myCart = myCart.Where(c => cartIds.Contains(c.Id)).ToList();

        if (!myCart.Any())
            return View("CheckoutCart", input);

        // Gán OrderDetails: LẤY GIÁ MỚI TỪ MOBILEPHONE SERVICE để tránh bị client thay đổi
        var orderDetails = new List<CreateOrderDetailDto>();
        foreach (var c in myCart.Where(x => x.Quantity > 0))
        {
            var phone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(c.MobilePhoneId));
            var unitPrice = GetEffectivePrice(phone);

            orderDetails.Add(new CreateOrderDetailDto
            {
                MobilePhoneId = c.MobilePhoneId,
                Quantity = c.Quantity,
                UnitPrice = unitPrice,
                MobilePhoneName = c.Name,
                ImageUrl = c.ImageUrl
            });
        }

        input.OrderDetails = orderDetails;
        input.DiscountCode = DiscountCode;
        input.PaymentMethod = PaymentMethod;

        // Lưu đơn hàng với trạng thái 0 (chờ thanh toán)
        input.Status = 0;
        var createdOrder = await _orderAppService.CreateAsync(input);

        // Xóa sản phẩm trong giỏ hàng đã thanh toán
        foreach (var cartId in cartIds)
            await _cartAppService.DeleteAsync(new EntityDto<int>(cartId));

        // Nếu chọn COD, cập nhật trạng thái thành 1 (thành công)
        if (PaymentMethod != 1) // COD
        {
            await _orderAppService.UpdateStatusAsync(createdOrder.Id, 1);
        }
        if (PaymentMethod == 1) // VNPAY
        {
            // Tạo Payment URL
            var paymentInfo = new PaymentInformationModel
            {
                Amount = (long)createdOrder.FinalAmount,
                OrderDescription = "Thanh toán đơn hàng qua VNPAY",
                OrderType = "other",
                Name = input.RecipientName ?? User.Identity?.Name ?? "Khách hàng",
                OrderId = createdOrder.Id.ToString()
            };

            var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);
            return Redirect(paymentUrl);
        }

        return RedirectToAction("Success");
    }

    [HttpGet]
    public async Task<IActionResult> PaymentCallbackVnpay()
    {
        var response = _vnPayService.PaymentExecute(Request.Query);

        var txnRef = Request.Query["vnp_TxnRef"].ToString();
        if (!int.TryParse(txnRef, out int orderId))
            return RedirectToAction("Fail");

        var vnpResponseCode = Request.Query["vnp_ResponseCode"].ToString();

        if (vnpResponseCode == "00") // thanh toán thành công
        {
            await _orderAppService.UpdateStatusAsync(orderId, 1);
            return RedirectToAction("Success");
        }
        else // thất bại hoặc hủy
        {
            await _orderAppService.UpdateStatusAsync(orderId, 3);
            return RedirectToAction("Fail");
        }
    }

    // ========================== Trạng thái thành công / thất bại ==========================
    public IActionResult Success() => View();
    public IActionResult Fail() => View();

    // ---------- Helper: tính giá thực (chỉ dùng DiscountPrice khi sale còn hiệu lực) ----------
    private decimal GetEffectivePrice(MobilePhoneDto phone)
    {
        if (phone == null) return 0m;

        // Mặc định không áp dụng sale
        var useSale = false;

        // Nếu có cờ IsOnSale thì ưu tiên kiểm tra
        try
        {
            if (phone.IsOnSale)
                useSale = true;
        }
        catch
        {
            // nếu MobilePhoneDto không có IsOnSale property, bỏ qua
        }

        // Chỉ dùng DiscountPrice khi phone.DiscountPrice < phone.Price và flag sale = true
        if (useSale && phone.DiscountPrice.HasValue && phone.DiscountPrice.Value < phone.Price)
            return phone.DiscountPrice.Value;

        // Mặc định trả về giá gốc
        return phone.Price;
    }
}
