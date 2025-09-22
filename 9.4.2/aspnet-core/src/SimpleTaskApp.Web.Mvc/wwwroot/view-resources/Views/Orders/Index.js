(function ($) {
    var _orderService = abp.services.app.order,
        l = abp.localization.getSource('SimpleTaskApp'),
        _$table = $('#OrdersTable');

    var _$ordersTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        processing: true,
        listAction: {
            ajaxFunction: _orderService.getAll
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
                        case 0: return l('Pending');
                        case 1: return l('Shipping');
                        case 2: return l('Completed');
                        case 3: return l('Cancelled');
                        default: return '-';
                    }
                }
            },
            {
                targets: 4,
                data: 'totalAmount',
                title: l('TotalAmount'),
                render: (data) => data ? data.toFixed(2) : ''
            },
            {
                targets: 5,
                data: null,
                sortable: false,
                title: l('Actions'),
                render: function (data, type, row) {
                    return `
                        <button type="button" 
                                class="btn btn-sm btn-info view-order" 
                                data-id="${row.id}">
                            <i class="fas fa-eye"></i> ${l('Detail')}
                        </button>
                        <button type="button" 
                                class="btn btn-sm btn-secondary edit-order" 
                                data-id="${row.id}">
                            <i class="fas fa-pencil-alt"></i> ${l('Edit')}
                        </button>
                        <button type="button" 
                                class="btn btn-sm btn-danger delete-order" 
                                data-id="${row.id}">
                            <i class="fas fa-trash"></i> ${l('Delete')}
                        </button>
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
