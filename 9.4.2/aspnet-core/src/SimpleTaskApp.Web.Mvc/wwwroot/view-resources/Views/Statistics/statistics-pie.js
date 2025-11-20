function initPieCharts(container) {
  $(container || document).find('.pie-chart').each(function () {
    var ctx = this.getContext('2d');
    var brands = JSON.parse($(this).attr('data-brands') || '[]');
    if (!brands.length) return;

    var labels = brands.map(b => b.BrandName);
    var data = brands.map(b => b.Revenue);
    var backgroundColors = labels.map(() => {
      var r = Math.floor(Math.random() * 156 + 100);
      var g = Math.floor(Math.random() * 156 + 100);
      var b = Math.floor(Math.random() * 156 + 100);
      return `rgba(${r},${g},${b},0.6)`;
    });

    new Chart(ctx, {
      type: 'pie',
      data: { labels, datasets: [{ data, backgroundColor: backgroundColors }] },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }
    });
  });
}

// Khi DOM load, render tất cả canvas có sẵn
document.addEventListener("DOMContentLoaded", function () {
  initPieCharts();
});