(function ($) {
    var _cartService = abp.services.app.cart,
        l = abp.localization.getSource('SimpleTaskApp'),
        _$table = $('#CartsTable');

    var _$cartsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        processing: true,
        listAction: {
            ajaxFunction: _cartService.getAll
        },
        buttons: [
            {
                name: 'refresh',
                text: `<i class="fas fa-redo-alt"></i> ${l('Refresh')}`,
                action: () => _$cartsTable.draw(false)
            }
        ],
        responsive: { details: { type: 'column' } },
        columnDefs: [
            { targets: 0, className: 'control', defaultContent: '' },
            { targets: 1, data: 'name', title: l('Name') },
            {
                targets: 2,
                data: 'price',
                title: l('Price'),
                render: (data) => data ? data.toFixed(2) : ''
            },
            {
                targets: 3,
                data: 'discountPrice',
                title: l('DiscountPrice'),
                render: (data) => data ? data.toFixed(2) : ''
            },
            {
                targets: 4,
                data: 'displayPrice',
                title: l('DisplayPrice'),
                render: (data) => data ? data.toFixed(2) : ''
            },
            { targets: 5, data: 'quantity', title: l('Quantity') },
            {
                targets: 6,
                data: 'total',
                title: l('Total'),
                render: (data) => data ? data.toFixed(2) : ''
            },
            {
                targets: 7,
                data: 'imageUrl',
                title: l('Image'),
                render: (data) =>
                    data ? `<img src="${data}" style="width:50px; height:50px;" />` : ''
            },
            {
                targets: 8,
                data: null,
                sortable: false,
                title: l('Actions'),
                render: (data, type, row) => `
        <div class="d-flex gap-2 justify-content-center">
            <button type="button" 
                    class="btn btn-sm btn-secondary edit-cart" 
                    data-id="${row.id}">
                <i class="fas fa-pencil-alt"></i> ${l('Edit')}
            </button>
            <button type="button" 
                    class="btn btn-sm btn-danger delete-cart" 
                    data-id="${row.id}" data-name="${row.name}">
                <i class="fas fa-trash"></i> ${l('Delete')}
            </button>
        </div>
    `
            }
        ]
    });

    // Edit Cart
    $(document).on('click', '.edit-cart', function () {
        var id = $(this).data('id');
        abp.ajax({
            url: abp.appPath + 'Admin/Carts/EditModal?Id=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#CartEditModal div.modal-content').html(content);
                $('#CartEditModal').modal('show');
            }
        });
    });

    abp.event.on('cart.edited', function () {
        _$cartsTable.ajax.reload();
    });

    // Delete Cart
    $(document).on('click', '.delete-cart', function () {
        var id = $(this).data('id');
        var name = $(this).data('name');

        abp.message.confirm(
            abp.utils.formatString(l('AreYouSureWantToDelete'), name),
            null,
            function (isConfirmed) {
                if (isConfirmed) {
                    _mobilePhoneService.delete({ id: id }).done(function () {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$mobilePhonesTable.ajax.reload();
                    });
                }
            }
        );
    });
})(jQuery);
