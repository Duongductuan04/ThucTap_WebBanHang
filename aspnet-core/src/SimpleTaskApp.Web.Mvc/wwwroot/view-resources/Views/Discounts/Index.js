(function ($) {
    var _discountService = abp.services.app.discount,
        l = abp.localization.getSource('SimpleTaskApp'),
        _$table = $('#DiscountsTable');

    // Khởi tạo DataTable
    var _$discountsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _discountService.getAll
        },
        buttons: [
            {
                name: 'refresh',
                text: `<i class="fas fa-redo-alt"></i> ${l('Refresh')}`,
                action: () => _$discountsTable.draw(false)
            },
         
        ],
        responsive: { details: { type: 'column' } },
        columnDefs: [
            { targets: 0, className: 'control', defaultContent: '' },
            { targets: 1, data: 'name', sortable: false, title: l('Name') },
            { targets: 2, data: 'code', sortable: false, title: l('Code') },
            {
                targets: 3,
                data: null,
                sortable: false,
                title: l('Discount'),
                render: function (data) {
                    return data.percentage ? data.percentage.toFixed(2) + '%' :
                        data.amount ? '$' + data.amount.toFixed(2) : '';
                }
            },
            {
                targets: 4,
                data: 'applyType',
                sortable: false,
                title: l('ApplyType'),
                render: function (data) {
                    switch (data) {
                        case 0: return l('All');
                        case 1: return l('Category');
                        case 2: return l('Product');
                        default: return '';
                    }
                }
            },
            {
                targets: 5,
                data: null,
                sortable: false,
                title: l('Duration'),
                render: function (data) {
                    return data.startDate + ' → ' + data.endDate;
                }
            },
            {
                targets: 6,
                data: 'isActive',
                sortable: false,
                title: l('IsActive'),
                render: function (data) { return data ? l('Yes') : l('No'); }
            },
            {
                targets: 7,
                data: null,
                sortable: false,
                title: l('Actions'),
                render: function (data, type, row) {
                    return `
            <div class="d-flex gap-2 justify-content-center">
                <button type="button" class="btn btn-sm btn-info detail-discount" data-id="${row.id}" data-toggle="modal" data-target="#DiscountDetailModal">
                    <i class="fas fa-info-circle"></i> ${l('Detail')}
                </button>
                <button type="button" class="btn btn-sm btn-secondary edit-discount" data-id="${row.id}" data-toggle="modal" data-target="#DiscountEditModal">
                    <i class="fas fa-pencil-alt"></i> ${l('Edit')}
                </button>
                <button type="button" class="btn btn-sm btn-danger delete-discount" data-id="${row.id}" data-name="${row.name}">
                    <i class="fas fa-trash"></i> ${l('Delete')}
                </button>
            </div>
        `;
                }
            }
            
        ]
    });

    // =================== CREATE ===================
    function showCreateModal() {
        abp.ajax({
            url: abp.appPath + 'Admin/Discounts/CreateModal',
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#DiscountCreateModal div.modal-content').html(content);
                $('#DiscountCreateModal').modal('show');
            }
        });
    }

    abp.event.on('discount.created', function () {
        _$discountsTable.ajax.reload();
    });
    // Bắt sự kiện click cho nút Create ngoài table
    $(document).on('click', '.create-discount', function () {
        showCreateModal();
    });
    // =================== EDIT ===================
    $(document).on('click', '.edit-discount', function () {
        var id = $(this).data('id');
        abp.ajax({
            url: abp.appPath + 'Admin/Discounts/EditModal?discountId=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#DiscountEditModal div.modal-content').html(content);
                $('#DiscountEditModal').modal('show');
            }
        });
    });

    abp.event.on('discount.edited', function () {
        _$discountsTable.ajax.reload();
    });

    // =================== DETAIL ===================
    $(document).on('click', '.detail-discount', function () {
        var id = $(this).data('id');
        abp.ajax({
            url: abp.appPath + 'Admin/Discounts/DetailModal?discountId=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#DiscountDetailModal div.modal-content').html(content);
                $('#DiscountDetailModal').modal('show');
            }
        });
    });
    // Mở modal khi click vào dòng, trừ cột Actions và control
    $('#DiscountsTable tbody').on('click', 'tr', function (e) {
        // Nếu click vào cột Actions hoặc control column thì không mở
        if (!$(e.target).closest('td').hasClass('control') &&
            !$(e.target).closest('td').is(':last-child')) { // cột Actions là cuối cùng
            var data = _$discountsTable.row(this).data();
            if (data) {
                openDiscountDetailModal(data.id);
            }
        }
    });

    // Hàm mở modal chi tiết Discount
    function openDiscountDetailModal(id) {
        abp.ajax({
            url: abp.appPath + 'Admin/Discounts/DetailModal?discountId=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#DiscountDetailModal div.modal-content').html(content);
                $('#DiscountDetailModal').modal('show');
            }
        });
    }
    // =================== DELETE ===================
    $(document).on('click', '.delete-discount', function () {
        var id = $(this).data('id');
        var name = $(this).data('name');

        abp.message.confirm(
            abp.utils.formatString(l('AreYouSureWantToDelete'), name),
            null,
            function (isConfirmed) {
                if (isConfirmed) {
                    _discountService.delete({ id: id }).done(function () {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$discountsTable.ajax.reload();
                    });
                }
            }
        );
    });
})(jQuery);
