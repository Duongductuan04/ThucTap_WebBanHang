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

  // ========================== OTP ==========================
  [IgnoreAntiforgeryToken]
  [HttpPost]
  public async Task<JsonResult> SendOtp([FromBody] SendOtpDto input)
  {
    if (input == null || string.IsNullOrWhiteSpace(input.PhoneNumber))
      return Json(new { success = false, message = "Số điện thoại không hợp lệ" });

    try
    {
      var result = await _otpService.SendOtpAsync(input);
      return Json(new
      {
        success = result.Success,
        message = result.Message,
        otpCode = result.OtpCode // chỉ dùng test/dev
      });
    }
    catch (Exception ex)
    {
      return Json(new { success = false, message = "Lỗi khi gửi OTP: " + ex.Message });
    }
  }

  [HttpGet]
  public async Task<IActionResult> CheckoutBuyNow(int mobilePhoneId, int quantity, int? mobilePhoneColorId = null)
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
                ImageUrl = mobilePhone.ImageUrl,
                MobilePhoneColorId = mobilePhoneColorId
            }
        }
    };

    orderDto.TotalAmount = orderDto.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
    orderDto.ShippingFee = 20000;
    orderDto.DiscountAmount = 0m;
    orderDto.FinalAmount = orderDto.TotalAmount + orderDto.ShippingFee;

    // Lấy danh sách màu
    var colors = await _mobilePhoneAppService.GetColorsByMobilePhoneIdAsync(mobilePhoneId);
    ViewBag.Colors = colors;

    // Gán màu vào OrderDetail nếu có
    if (mobilePhoneColorId.HasValue)
    {
      var selectedColor = colors.FirstOrDefault(c => c.Id == mobilePhoneColorId.Value);
      if (selectedColor != null)
      {
        var orderDetail = orderDto.OrderDetails.First();
        orderDetail.ColorName = selectedColor.ColorName;
        orderDetail.ColorImageUrl = selectedColor.ImageUrl;
      }
    }

    // Lấy ưu đãi
    ViewBag.AvailableDiscounts = await _orderAppService.GetAvailableDiscountsAsync(
        orderDto.OrderDetails.Select(od => new OrderItemDto
        {
          ProductId = od.MobilePhoneId,
          Quantity = od.Quantity,
          UnitPrice = od.UnitPrice
        }).ToList(),
        orderDto.TotalAmount
    );

    ViewBag.SelectedColorId = mobilePhoneColorId;

    return View("CheckoutBuyNow", orderDto);
  }

  [HttpGet]
  public async Task<IActionResult> CheckoutCart(List<int> cartIds)
  {
    var myCart = await _cartAppService.GetMyCartAsync() ?? new List<CartDto>();
    var selectedItems = myCart.Where(c => cartIds.Contains(c.Id)).ToList();
    if (!selectedItems.Any()) return RedirectToAction("Index", "Carts");

    var orderDetails = selectedItems.Select(c => new CreateOrderDetailDto
    {
      MobilePhoneId = c.MobilePhoneId,
      Quantity = c.Quantity,
      UnitPrice = c.DisplayPrice,          // ✅ dùng DisplayPrice đã tính sẵn
      MobilePhoneName = c.Name,
      ImageUrl = c.ImageUrl,
      MobilePhoneColorId = c.MobilePhoneColorId,
      ColorName = c.ColorName,             // ✅ lấy trực tiếp từ CartDto
      ColorImageUrl = c.ColorImageUrl      // ✅ lấy trực tiếp từ CartDto
    }).ToList();

    var orderDto = new CreateOrderDto
    {
      OrderDetails = orderDetails,
      TotalAmount = orderDetails.Sum(od => od.Quantity * od.UnitPrice),
      ShippingFee = 20000,
      DiscountAmount = 0m
    };

    orderDto.FinalAmount = orderDto.TotalAmount + orderDto.ShippingFee;

    // Lấy ưu đãi
    var availableDiscounts = await _orderAppService.GetAvailableDiscountsAsync(
        orderDetails.Select(od => new OrderItemDto
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

  // ========================== Thanh toán mua ngay ==========================
  [HttpPost]
  public async Task<IActionResult> BuyNowPost(
      CreateOrderDto input,
      [FromForm] int MobilePhoneId,
      [FromForm] int Quantity,
      [FromForm] int? MobilePhoneColorId,  // 👈 thêm màu sắc
      [FromForm] string DiscountCode,
      [FromForm] int PaymentMethod,
      [FromForm] string OtpCode)
  {
    var verifyDto = new VerifyOtpDto
    {
      PhoneNumber = input.RecipientPhone,
      OtpCode = OtpCode
    };

    var otpResult = await _otpService.VerifyOtpAsync(verifyDto);
    if (!otpResult.Success)
    {
      TempData["Error"] = otpResult.Message;
      return RedirectToAction("CheckoutBuyNow", new { mobilePhoneId = MobilePhoneId, quantity = Quantity });
    }

    var mobilePhone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(MobilePhoneId));
    if (mobilePhone == null)
      return RedirectToAction("CheckoutBuyNow", new { mobilePhoneId = MobilePhoneId, quantity = Quantity });

    var unitPrice = GetEffectivePrice(mobilePhone);

    input.OrderDetails = new List<CreateOrderDetailDto>
        {
            new CreateOrderDetailDto
            {
                MobilePhoneId = mobilePhone.Id,
                Quantity = Quantity,
                UnitPrice = unitPrice,
                MobilePhoneName = mobilePhone.Name,
                ImageUrl = mobilePhone.ImageUrl,
                MobilePhoneColorId = MobilePhoneColorId // 👈 lưu màu sắc
            }
        };

    input.DiscountCode = DiscountCode;
    input.PaymentMethod = PaymentMethod;
    input.Status = 0; // chờ thanh toán

    var createdOrder = await _orderAppService.CreateAsync(input);

    if (PaymentMethod != 1)
      await _orderAppService.UpdateStatusAsync(createdOrder.Id, 1);
    else
    {
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

  // ========================== Thanh toán giỏ hàng ==========================
  [HttpPost]
  public async Task<IActionResult> CheckoutCartPost(
      CreateOrderDto input,
      [FromForm] List<int> cartIds,
      [FromForm] string DiscountCode,
      [FromForm] int PaymentMethod,
      [FromForm] string OtpCode)
  {
    var verifyDto = new VerifyOtpDto
    {
      PhoneNumber = input.RecipientPhone,
      OtpCode = OtpCode
    };

    var otpResult = await _otpService.VerifyOtpAsync(verifyDto);
    if (!otpResult.Success)
    {
      TempData["Error"] = otpResult.Message;
      return RedirectToAction("CheckoutCart", new { cartIds });
    }

    cartIds ??= new List<int>();
    var myCart = await _cartAppService.GetMyCartAsync() ?? new List<CartDto>();
    if (cartIds.Any())
      myCart = myCart.Where(c => cartIds.Contains(c.Id)).ToList();

    if (!myCart.Any())
      return View("CheckoutCart", input);

    var orderDetails = new List<CreateOrderDetailDto>();

    for (int i = 0; i < myCart.Count; i++)
    {
      var c = myCart[i];
      var phone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(c.MobilePhoneId));
      var unitPrice = GetEffectivePrice(phone);

      orderDetails.Add(new CreateOrderDetailDto
      {
        MobilePhoneId = c.MobilePhoneId,
        Quantity = c.Quantity,
        UnitPrice = unitPrice,
        MobilePhoneName = c.Name,
        ImageUrl = c.ImageUrl,
        MobilePhoneColorId = c.MobilePhoneColorId
      });
    }
    input.OrderDetails = orderDetails;
    input.DiscountCode = DiscountCode;
    input.PaymentMethod = PaymentMethod;
    input.Status = 0;

    var createdOrder = await _orderAppService.CreateAsync(input);

    foreach (var cartId in cartIds)
      await _cartAppService.DeleteAsync(new EntityDto<int>(cartId));

    if (PaymentMethod != 1)
      await _orderAppService.UpdateStatusAsync(createdOrder.Id, 1);
    else
    {
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

  // ========================== Callback VNPAY ==========================
  [HttpGet]
  public async Task<IActionResult> PaymentCallbackVnpay()
  {
    var response = _vnPayService.PaymentExecute(Request.Query);
    var txnRef = Request.Query["vnp_TxnRef"].ToString();
    if (!int.TryParse(txnRef, out int orderId))
      return RedirectToAction("Fail");

    var vnpResponseCode = Request.Query["vnp_ResponseCode"].ToString();
    if (vnpResponseCode == "00")
    {
      await _orderAppService.UpdateStatusAsync(orderId, 1);
      return RedirectToAction("Success");
    }
    else
    {
      await _orderAppService.UpdateStatusAsync(orderId, 3);
      return RedirectToAction("Fail");
    }
  }

  // ========================== Success / Fail ==========================
  public IActionResult Success() => View();
  public IActionResult Fail() => View();

  // ---------- Helper ----------
  private decimal GetEffectivePrice(MobilePhoneDto phone)
  {
    if (phone == null) return 0m;
    var useSale = false;

    try { if (phone.IsOnSale) useSale = true; } catch { }

    if (useSale && phone.DiscountPrice.HasValue && phone.DiscountPrice.Value < phone.Price)
      return phone.DiscountPrice.Value;

    return phone.Price;
  }
}
