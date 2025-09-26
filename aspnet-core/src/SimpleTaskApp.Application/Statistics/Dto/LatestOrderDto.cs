using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Statistics.Dto
{
    public class LatestOrderDto
    {
        public int OrderId { get; set; }             // Mã đơn hàng
        public int Status { get; set; }           // Trạng thái đơn
        public decimal TotalAmount { get; set; }     // Tổng tiền đơn
        public DateTime CreationTime { get; set; }   // Ngày tạo đơn
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>(); // Danh sách sản phẩm
    }

}
