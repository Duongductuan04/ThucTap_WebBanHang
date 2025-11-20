$(function () {
  'use strict';

  // === Khởi tạo giá trị mặc định của tháng hiện tại ===
  var _selectedDateRange = {
    startDate: null,
    endDate: null
  };

  var filterForm = $('#filterForm');
  var select = $('#statisticSelect');
  var container = $('#statisticContainer');
  var statsContent = $('#statsContent'); // phần info boxes + biểu đồ

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
      monthNames: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6', 'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'],
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

  // === Hàm load thống kê ===
  function loadStatistics() {
    const type = select.val();

    if (!type) {
      // Nếu không chọn thống kê → hiện lại stats
      statsContent.show();
      container.html('');
      return;
    }

    // Nếu chọn thống kê → ẩn stats
    statsContent.hide();

    let startDate = _selectedDateRange.startDate || '';
    let endDate = _selectedDateRange.endDate || '';

    // Nếu loại thống kê là LowStock → hủy thời gian
    if (type === 'LowStock') {
      startDate = '';
      endDate = '';
      _selectedDateRange.startDate = null;
      _selectedDateRange.endDate = null;
      $('#StartEndRange').val(''); // xóa hiển thị
    }

    fetch(`/Admin/Statistics/LoadPartial?type=${type}&StartDate=${startDate}&EndDate=${endDate}`)
      .then(res => res.text())
      .then(html => {
        container.html(html);
        if (type === "BrandRevenue" && typeof initPieCharts === "function") {
          initPieCharts(container);
        }
      });
  }

  // === Nhấn nút Thống kê mới load ===
  $('#btnLoadStatistics').on('click', function () {
    loadStatistics();
  });

});
