using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SimpleTaskApp.Vnpay;
using SimpleTaskApp.Vnpay;
using Abp.AspNetCore.Mvc.Controllers;

namespace SimpleTaskApp.Web.Controllers
{
    public class PaymentController : AbpController
    {

        private readonly IVnPayService _vnPayService;
        public PaymentController(IVnPayService vnPayService)
        {

            _vnPayService = vnPayService;
        }
        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
         

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }


    }
}
