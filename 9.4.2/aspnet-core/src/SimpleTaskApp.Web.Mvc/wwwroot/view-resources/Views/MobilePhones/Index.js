(function ($) {
    var _mobilePhoneService = abp.services.app.mobilePhone,
        l = abp.localization.getSource('SimpleTaskApp'),
        _$table = $('#MobilePhonesTable');

    // Khởi tạo DataTable
    var _$mobilePhonesTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _mobilePhoneService.getAll
        },
        buttons: [
            {
                name: 'refresh',
                text: `<i class="fas fa-redo-alt"></i> ${l('Refresh')}`,
                action: () => _$mobilePhonesTable.draw(false)
            }
        ],
        responsive: { details: { type: 'column' } },
        columnDefs: [
            { targets: 0, className: 'control', defaultContent: '' },
            { targets: 1, data: 'name', sortable: false, title: l('Name') },
            {
                targets: 2,
                data: 'price',
                sortable: false,
                title: l('Price'),
                render: function (data) {
                    return data ? data.toFixed(2) : '';
                }
            },
            {
                targets: 3,
                data: 'discountPrice',
                sortable: false,
                title: l('DiscountPrice'),
                render: function (data) {
                    return data ? data.toFixed(2) : '';
                }
            },
            { targets: 4, data: 'brand', sortable: false, title: l('Brand') },
            {
                targets: 5,
                data: 'imageUrl',
                sortable: false,
                title: l('Image'),
                render: function (data) {
                    return data ? `<img src="${data}" style="width:50px; height:50px;" />` : '';
                }
            },
            {
                targets: 6,
                data: null,
                sortable: false,
                title: l('Actions'),
                render: function (data, type, row) {
                    return `
                        <button type="button" class="btn btn-sm btn-info detail-mobilephone" data-id="${row.id}" data-toggle="modal" data-target="#MobilePhoneDetailModal">
                            <i class="fas fa-info-circle"></i> ${l('Detail')}
                        </button>
                        <button type="button" class="btn btn-sm btn-secondary edit-mobilephone" data-id="${row.id}" data-toggle="modal" data-target="#MobilePhoneEditModal">
                            <i class="fas fa-pencil-alt"></i> ${l('Edit')}
                        </button>
                        <button type="button" class="btn btn-sm btn-danger delete-mobilephone" data-id="${row.id}" data-name="${row.name}">
                            <i class="fas fa-trash"></i> ${l('Delete')}
                        </button>
                    `;
                }
            }
        ]
    });

    // Detail MobilePhone
    $(document).on('click', '.detail-mobilephone', function () {
        var id = $(this).data('id');
        abp.ajax({
            url: abp.appPath + 'Admin/MobilePhones/DetailModal?mobilePhoneId=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#MobilePhoneDetailModal div.modal-content').html(content);
            }
        });
    });

    // Create MobilePhone
    $(document).on('click', '.create-mobilephone', function () {
        abp.ajax({
            url: abp.appPath + 'Admin/MobilePhones/CreateModal',
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#MobilePhoneCreateModal div.modal-content').html(content);
                $('#MobilePhoneCreateModal').modal('show');
            }
        });
    });

    abp.event.on('mobilephone.created', function () {
        _$mobilePhonesTable.ajax.reload();
    });

    // Edit MobilePhone
    $(document).on('click', '.edit-mobilephone', function () {
        var id = $(this).data('id');
        abp.ajax({
            url: abp.appPath + 'Admin/MobilePhones/EditModal?mobilePhoneId=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#MobilePhoneEditModal div.modal-content').html(content);
            }
        });
    });

    abp.event.on('mobilephone.edited', function () {
        _$mobilePhonesTable.ajax.reload();
    });

    // Delete MobilePhone
    $(document).on('click', '.delete-mobilephone', function () {
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
