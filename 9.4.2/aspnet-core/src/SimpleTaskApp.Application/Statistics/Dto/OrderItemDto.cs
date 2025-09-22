using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Statistics.Dto
{
    public class OrderItemDto
    {
        public string MobilePhoneName { get; set; }  // Tên sản phẩm
        public int Quantity { get; set; }            // Số lượng
        public decimal UnitPrice { get; set; }       // Giá 1 sản phẩm
        public decimal TotalPrice => Quantity * UnitPrice; // Tổng tiền sản phẩm
    }

}
