$(function () {
    'use strict';

    // Lấy context của canvas
    var ctx = document.getElementById('revenueChart').getContext('2d');

    // Lấy dữ liệu doanh thu từ view (model)
    var revenues = $(ctx.canvas).data('revenues'); // set data attribute trong cshtml

    // Tạo biểu đồ cột
    var revenueChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: [
                'Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
                'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'
            ],
            datasets: [{
                label: 'Doanh thu (VNĐ)',
                data: revenues,
                backgroundColor: [
                    'rgba(255, 99, 132, 0.6)',
                    'rgba(54, 162, 235, 0.6)',
                    'rgba(255, 206, 86, 0.6)',
                    'rgba(75, 192, 192, 0.6)',
                    'rgba(153, 102, 255, 0.6)',
                    'rgba(255, 159, 64, 0.6)',
                    'rgba(199, 199, 199, 0.6)',
                    'rgba(83, 102, 255, 0.6)',
                    'rgba(255, 99, 71, 0.6)',
                    'rgba(60, 179, 113, 0.6)',
                    'rgba(255, 215, 0, 0.6)',
                    'rgba(123, 104, 238, 0.6)'
                ],
                borderColor: 'rgba(0,0,0,0.8)',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value.toLocaleString('vi-VN') + ' đ';
                        }
                    }
                }
            }
        }
    });
});
