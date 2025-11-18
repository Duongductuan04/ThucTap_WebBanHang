$(function () {
    $('.pie-chart').each(function () {
        var ctx = this.getContext('2d');
        var brands = $(this).data('brands');

        var labels = brands.map(b => b.BrandName);
        var data = brands.map(b => b.Revenue);

        // Tạo màu động cho mỗi brand
        var backgroundColors = labels.map(() => {
            // random màu pastel
            var r = Math.floor(Math.random() * 156) + 100; // 100-255
            var g = Math.floor(Math.random() * 156) + 100;
            var b = Math.floor(Math.random() * 156) + 100;
            return `rgba(${r}, ${g}, ${b}, 0.6)`;
        });

        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: backgroundColors
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false, // để canvas resize theo div
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    });
});
