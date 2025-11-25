$(function () {
  'use strict';

  // === Khởi tạo giá trị mặc định của tháng hiện tại ===
  var _selectedDateRange = { startDate: null, endDate: null };

  var filterForm = $('#filterForm');
  var select = $('#statisticSelect');
  var container = $('#statisticContainer');
  var statsContent = $('#statsContent'); // info boxes + biểu đồ

  // === Khởi tạo Date Range Picker với các ranges mặc định ===
  $('#StartEndRange').daterangepicker({
    autoUpdateInput: false,
    opens: 'left',
    maxDate: moment(),
    locale: {
      format: 'DD/MM/YYYY',
      applyLabel: 'Áp dụng',
      cancelLabel: 'Hủy',
      customRangeLabel: 'Tùy chỉnh',
      daysOfWeek: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'],
      monthNames: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
        'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'],
      firstDay: 1
    },
    ranges: {
      'Hôm nay': [moment(), moment()],
      'Hôm qua': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
      '7 ngày qua': [moment().subtract(6, 'days'), moment()],
      '1 tháng trước': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
      '3 tháng trước': [moment().subtract(3, 'months').startOf('month'), moment()],
      '6 tháng qua': [moment().subtract(6, 'months'), moment()],
      'Năm nay': [moment().startOf('year'), moment()]
    }
  }, function (start, end) {
    _selectedDateRange.startDate = start.format('YYYY-MM-DDT00:00:00');
    _selectedDateRange.endDate = end.format('YYYY-MM-DDT23:59:59');
    $('#StartEndRange').val(start.format('DD/MM/YYYY') + ' - ' + end.format('DD/MM/YYYY'));
  });

  // === Khi nhấn hủy ===
  $('#StartEndRange').on('cancel.daterangepicker', function () {
    $(this).val('');
    _selectedDateRange.startDate = null;
    _selectedDateRange.endDate = null;
  });
  // Khi input bị xóa (trống) thì reset giống nút hủy
  $('#StartEndRange').on('input', function () {
    if ($(this).val().trim() === '') {
      _selectedDateRange.startDate = null;
      _selectedDateRange.endDate = null;
    }
  });


  // ==============================
  // Hàm load thống kê
  // ==============================
  function loadStatistics() {
    const type = select.val();
    // Nếu chọn LowStock → reset thời gian
    if (type === "LowStock") {
      _selectedDateRange.startDate = null;
      _selectedDateRange.endDate = null;
      $('#StartEndRange').val(''); // xóa input hiển thị
    }

    let startDate = _selectedDateRange.startDate;
    let endDate = _selectedDateRange.endDate;

    // Overview → hiển thị info boxes + biểu đồ
    if (type === "Overview") {
      statsContent.show();
    } else {
      statsContent.hide();
    }

    // Tạo URL với filter thời gian nếu có
    let url = `/Admin/Statistics/LoadPartial?type=${type}`;
    if (type !== "LowStock") {
      if (startDate) url += `&StartDate=${startDate}`;
      if (endDate) url += `&EndDate=${endDate}`;
    }

    fetch(url)
      .then(res => res.text())
      .then(html => {
        container.html(html);
        // Khởi tạo pie chart nếu là BrandRevenue
        if (type === "BrandRevenue" && typeof initPieCharts === "function") {
          initPieCharts(container);
        }
        // Khởi tạo chart doanh thu nếu Overview
        if (type === "Overview" && typeof renderRevenueChart === "function") {
          renderRevenueChart();
        }
      });
  }

  // ==============================
  // Nhấn nút Thống kê
  // ==============================
  $('#btnLoadStatistics').on('click', function () {
    loadStatistics();
  });

});