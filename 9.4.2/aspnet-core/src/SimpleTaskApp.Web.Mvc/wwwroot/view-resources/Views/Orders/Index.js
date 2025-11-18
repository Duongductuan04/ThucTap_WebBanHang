(function ($) {
    var _orderService = abp.services.app.order,
        l = abp.localization.getSource('SimpleTaskApp'),
        _$table = $('#OrdersTable');

    var _$ordersTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        processing: true,
        listAction: {
            ajaxFunction: _orderService.getAll,
            inputFilter: function () {
                return $('#OrdersSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: `<i class="fas fa-redo-alt"></i> ${l('Refresh')}`,
                action: () => _$ordersTable.draw(false)
            }
        ],
        responsive: { details: { type: 'column' } },
        columnDefs: [
            { targets: 0, className: 'control', defaultContent: '' },
            { targets: 1, data: 'recipientName', title: l('Recipient') },
            { targets: 2, data: 'recipientPhone', title: l('Phone') },
            {
                targets: 3,
                data: 'status',
                title: l('Status'),
                render: function (data) {
                    switch (data) {
                        case 0: return `<span class="badge bg-warning">${l('Pending')}</span>`;
                        case 1: return `<span class="badge bg-info">${l('Shipping')}</span>`;
                        case 2: return `<span class="badge bg-success">${l('Completed')}</span>`;
                        case 3: return `<span class="badge bg-danger">${l('Cancelled')}</span>`;
                        default: return '-';
                    }
                }
            },
            {
                targets: 4,
                data: 'totalAmount',
                title: l('TotalAmount'),
                render: (data) => data
                    ? data.toLocaleString('vi-VN') + ' đ'
                    : '0 đ'
            },
            {
                targets: 5,
                data: 'shippingFee',
                title: l('ShippingFee'),
                render: (data) => data
                    ? data.toLocaleString('vi-VN') + ' đ'
                    : '0 đ'
            },
            {
                targets: 6,
                data: 'finalAmount',
                title: l('FinalAmount'),
                render: (data) => data
                    ? `<strong class="text-danger">${data.toLocaleString('vi-VN')} đ</strong>`
                    : '0 đ'
            },
          {
            targets: 7,
            data: null,
            sortable: false,
            title: l('Actions'),
            render: function (data, type, row) {
              return `
<div class="d-flex justify-content-around flex-wrap">
    <button type="button" class="btn btn-sm btn-info view-order" data-id="${row.id}" title="${l('Detail')}">
        <i class="fas fa-eye"></i>
    </button>
    <button type="button" class="btn btn-sm btn-secondary edit-order" data-id="${row.id}" title="${l('Edit')}">
        <i class="fas fa-edit"></i>
    </button>
    <button type="button" class="btn btn-sm btn-danger delete-order" data-id="${row.id}" title="${l('Delete')}">
        <i class="fas fa-times"></i>
    </button>
    <a href="${abp.appPath}Admin/Orders/PrintInvoice?id=${row.id}" 
       class="btn btn-sm btn-success" target="_blank" title="${l('Print')}">
       <i class="fas fa-print"></i>
    </a>
</div>
        `;
            }
          }

        ]
    });

    // View Order Detail
    $(document).on('click', '.view-order', function () {
        var id = $(this).data('id');
        abp.ajax({
            url: abp.appPath + 'Admin/Orders/DetailModal?Id=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#OrderDetailModal div.modal-content').html(content);
                $('#OrderDetailModal').modal('show');
            }
        });
    });
    // Hoặc: click toàn bộ dòng trừ cột Actions
    $('#OrdersTable tbody').on('click', 'tr', function (e) {
        if (!$(e.target).closest('td').hasClass('control') &&
            !$(e.target).closest('td').is(':last-child')) { // cột Actions là cuối cùng
            var data = _$ordersTable.row(this).data();
            if (data) {
                openOrderDetailModal(data.id);
            }
        }
    });

    // Hàm mở modal chi tiết Order
    function openOrderDetailModal(id) {
        abp.ajax({
            url: abp.appPath + 'Admin/Orders/DetailModal?Id=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#OrderDetailModal div.modal-content').html(content);
                $('#OrderDetailModal').modal('show');
            }
        });
    }
    // Edit Order
    $(document).on('click', '.edit-order', function () {
        var id = $(this).data('id');
        abp.ajax({
            url: abp.appPath + 'Admin/Orders/EditModal?Id=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#OrderEditModal div.modal-content').html(content);
                $('#OrderEditModal').modal('show');
            }
        });
    });
    // ======= Search nâng cao =======
    $('.btn-search').on('click', () => {
        _$ordersTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$ordersTable.ajax.reload();
            return false;
        }
    });
    abp.event.on('order.edited', function () {
        _$ordersTable.ajax.reload();
    });

    // Delete Order
    $(document).on('click', '.delete-order', function () {
        var id = $(this).data('id');

        abp.message.confirm(
            l('AreYouSureWantToDelete'),
            null,
            function (isConfirmed) {
                if (isConfirmed) {
                    _orderService.delete({ id: id }).done(function () {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$ordersTable.ajax.reload();
                    });
                }
            }
        );
    });
})(jQuery);
