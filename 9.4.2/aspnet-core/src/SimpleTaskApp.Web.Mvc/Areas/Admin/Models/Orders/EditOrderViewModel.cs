using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;

namespace SimpleTaskApp.Areas.Admin.Models.Orders
{
    public class EditOrderViewModel
    {
        public OrderDto Order { get; set; }

    }
}
