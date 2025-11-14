(function ($) {
  var _importService = abp.services.app.import,
    l = abp.localization.getSource('SimpleTaskApp'),
    _$table = $('#ImportsTable');

  // =================== Khởi tạo DataTable ===================
  var _$importsTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    listAction: {
      ajaxFunction: _importService.getAll,
      inputFilter: function () {
        return $('#ImportsSearchForm').serializeFormToObject(true);
      }
    },
    buttons: [
      {
        name: 'refresh',
        text: `<i class="fas fa-redo-alt"></i> ${l('Refresh')}`,
        action: () => _$importsTable.draw(false)
      }
    ],
    responsive: { details: { type: 'column' } },
    columnDefs: [
      { targets: 0, className: 'control', defaultContent: '' },
      { targets: 1, data: 'importCode', sortable: false, title: l('ImportCode') },
      { targets: 2, data: 'supplierName', sortable: false, title: l('SupplierName') },
      { targets: 3, data: 'keeperName', sortable: false, title: l('KeeperName') },
      { targets: 4, data: 'keeperPhone', sortable: false, title: l('KeeperPhone') },
      {
        targets: 5,
        data: 'creationTime',
        sortable: false,
        title: l('CreationTime'),
        render: function (data) {
          return data ? moment(data).format('YYYY-MM-DD HH:mm') : '';
        }
      },
      {
        targets: 6,
        data: null,
        sortable: false,
        title: l('Actions'),
        render: function (data, type, row) {
          return `
                        <div class="d-flex gap-2 justify-content-center">
                            <button type="button" class="btn btn-sm btn-info detail-import" data-id="${row.id}">
                                <i class="fas fa-info-circle"></i> ${l('Detail')}
                            </button>
                            <button type="button" class="btn btn-sm btn-secondary edit-import" data-id="${row.id}">
                                <i class="fas fa-pencil-alt"></i> ${l('Edit')}
                            </button>
                            <button type="button" class="btn btn-sm btn-danger delete-import" data-id="${row.id}" data-name="${row.importCode}">
                                <i class="fas fa-trash"></i> ${l('Delete')}
                            </button>
                        </div>
                    `;
        }
      }
    ]
  });

  // =================== CREATE ===================
  $(document).on('click', '.create-import', function () {
    abp.ajax({
      url: abp.appPath + 'Admin/Imports/CreateModal',
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#ImportCreateModal div.modal-content').html(content);
        $('#ImportCreateModal').modal('show');
      }
    });
  });
  abp.event.on('import.created', function () {
    _$importsTable.ajax.reload();
  });

  // =================== EDIT ===================
  $(document).on('click', '.edit-import', function () {
    var id = $(this).data('id');
    abp.ajax({
      url: abp.appPath + 'Admin/Imports/EditModal?importId=' + id,
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#ImportEditModal div.modal-content').html(content);
        $('#ImportEditModal').modal('show');
      }
    });
  });
  abp.event.on('import.edited', function () {
    _$importsTable.ajax.reload();
  });

  // =================== DETAIL ===================
  function openImportDetailModal(id) {
    abp.ajax({
      url: abp.appPath + 'Admin/Imports/DetailModal?importId=' + id,
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#ImportDetailModal div.modal-content').html(content);
        $('#ImportDetailModal').modal('show');
      }
    });
  }

  $(document).on('click', '.detail-import', function () {
    var id = $(this).data('id');
    openImportDetailModal(id);
  });

  // Mở modal khi click vào row (ngoại trừ Actions & control)
  $('#ImportsTable tbody').on('click', 'tr', function (e) {
    if (!$(e.target).closest('td').hasClass('control') &&
      !$(e.target).closest('td').is(':last-child')) {
      var data = _$importsTable.row(this).data();
      if (data) openImportDetailModal(data.id);
    }
  });

  // =================== DELETE ===================
  $(document).on('click', '.delete-import', function () {
    var id = $(this).data('id');
    var name = $(this).data('name');

    abp.message.confirm(
      abp.utils.formatString(l('AreYouSureWantToDelete'), name),
      null,
      function (isConfirmed) {
        if (isConfirmed) {
          _importService.delete({ id: id }).done(function () {
            abp.notify.info(l('SuccessfullyDeleted'));
            _$importsTable.ajax.reload();
          });
        }
      }
    );
  });

  // =================== SEARCH ===================
  $('#ImportsSearchForm .btn-search').on('click', function () {
    _$importsTable.ajax.reload();
  });
  $('#ImportsSearchForm .txt-search').on('keypress', function (e) {
    if (e.which == 13) {
      _$importsTable.ajax.reload();
      return false;
    }
  });
  $('#ImportsSearchForm .btn-clear').on('click', function () {
    $('#ImportsSearchForm')[0].reset();
    _$importsTable.ajax.reload();
  });

})(jQuery);
