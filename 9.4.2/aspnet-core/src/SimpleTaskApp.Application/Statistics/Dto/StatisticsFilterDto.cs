using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Statistics.Dto
{
    public class StatisticsFilterDto
    {
        public int? Day { get; set; }     // Ngày cụ thể
        public int? Month { get; set; }   // Tháng
        public int? Year { get; set; }    // Năm
    }
}
