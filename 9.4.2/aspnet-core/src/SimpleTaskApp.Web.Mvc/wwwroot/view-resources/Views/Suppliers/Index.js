(function ($) {

  const _supplierService = abp.services.app.supplier,
    l = abp.localization.getSource('SimpleTaskApp'),
    _$table = $('#SuppliersTable');

  // =================== DATATABLE ===================
  const _$suppliersTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    listAction: {
      ajaxFunction: _supplierService.getAll,
      inputFilter: function () {
        return $('#SuppliersSearchForm').serializeFormToObject(true);
      }
    },
    buttons: [
      {
        name: 'refresh',
        text: `<i class="fas fa-redo-alt"></i> ${l("Refresh")}`,
        action: () => _$suppliersTable.draw(false)
      }
    ],
    responsive: { details: { type: 'column' } },
    columnDefs: [
      { targets: 0, className: 'control', defaultContent: '' }, // Id column
      { targets: 1, data: 'supplierCode', title: l('SupplierCode'), sortable: false },
      { targets: 2, data: 'supplierName', title: l('SupplierName'), sortable: false },
      { targets: 3, data: 'phone', title: l('Phone'), sortable: false },
      { targets: 4, data: 'email', title: l('Email'), sortable: false },
      {
        targets: 5,
        data: 'isActive',
        title: l('Status'),
        sortable: false,
        render: function (data) {
          return data ? l("Active") : l("Inactive");
        }
      },
      {
        targets: 6,
        data: null,
        sortable: false,
        title: l('Actions'),
        render: function (data, type, row) {
          console.log('Row data:', row);

          return `
                        <div class="d-flex justify-content-around">
                            <button type="button"
                                class="btn btn-sm btn-secondary edit-supplier"
                                data-id="${row.id}" title="${l('Edit')}">
                                <i class="fas fa-edit"></i>
                            </button>

                            <button type="button"
                                class="btn btn-sm btn-danger delete-supplier"
                                data-id="${row.id}" data-name="${row.supplierName}"
                                title="${l('Delete')}">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>`;
        }
      }
    ]
  });

  // =================== CREATE ===================
  $(document).on('click', '.create-supplier', function () {
    abp.ajax({
      url: abp.appPath + 'Admin/Suppliers/CreateModal',
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#SupplierCreateModal div.modal-content').html(content);
        $('#SupplierCreateModal').modal('show');
      }
    });
  });

  abp.event.on('supplier.created', function () {
    _$suppliersTable.ajax.reload();
  });

  // =================== EDIT ===================
  $(document).on('click', '.edit-supplier', function () {
    const id = $(this).data('id');

    abp.ajax({
      url: abp.appPath + 'Admin/Suppliers/EditModal?id=' + id,
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#SupplierEditModal div.modal-content').html(content);
        $('#SupplierEditModal').modal('show');
      }
    });
  });

  abp.event.on('supplier.edited', function () {
    _$suppliersTable.ajax.reload();
  });

  // =================== DELETE ===================
  $(document).on('click', '.delete-supplier', function () {
    const id = $(this).data('id');
    const name = $(this).data('name');

    abp.message.confirm(
      abp.utils.formatString(l("AreYouSureWantToDelete"), name),
      null,
      function (isConfirmed) {
        if (isConfirmed) {
          _supplierService.delete(id).done(function () {
            abp.notify.info(l("SuccessfullyDeleted"));
            _$suppliersTable.ajax.reload();
          });
        }
      }
    );
  });

  // =================== SEARCH ===================
  $('#SuppliersSearchForm .btn-search').on('click', function () {
    _$suppliersTable.ajax.reload();
  });

  $('#SuppliersSearchForm .txt-search').on('keypress', function (e) {
    if (e.which === 13) {
      _$suppliersTable.ajax.reload();
      return false;
    }
  });

  $('#SuppliersSearchForm .btn-clear').on('click', function () {
    $('#SuppliersSearchForm')[0].reset();
    _$suppliersTable.ajax.reload();
  });

  // =================== DETAIL ===================
  // Chỉ mở Detail khi click vào row, trừ cột hành động
  $('#SuppliersTable tbody').on('click', 'tr', function (e) {
    // Nếu click vào button trong cột hành động thì return
    if ($(e.target).closest('.edit-supplier, .delete-supplier').length) {
      return;
    }
    const data = _$suppliersTable.row(this).data();
    if (!data) return;

    abp.ajax({
      url: abp.appPath + 'Admin/Suppliers/DetailModal?id=' + data.id,
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        // Xóa modal cũ nếu còn
        $('#SupplierDetailModal').remove();
        // Append modal mới
        $('body').append('<div class="modal fade" id="SupplierDetailModal" tabindex="-1" role="dialog" data-backdrop="static"><div class="modal-dialog"><div class="modal-content"></div></div></div>');
        $('#SupplierDetailModal div.modal-content').html(content);
        $('#SupplierDetailModal').modal('show');
      }
    });
  });

})(jQuery);
