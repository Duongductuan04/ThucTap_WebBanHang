(function ($) {

  const _categoryService = abp.services.app.mobilePhoneCategory,
    l = abp.localization.getSource('SimpleTaskApp'),
    _$table = $('#CategoriesTable');

  // =================== DATATABLE ===================
  const _$categoriesTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    listAction: {
      ajaxFunction: _categoryService.getAll,
      inputFilter: function () {
        return $('#CategoriesSearchForm').serializeFormToObject(true);
      }
    },
    buttons: [
      {
        name: 'refresh',
        text: `<i class="fas fa-redo-alt"></i> ${l("Refresh")}`,
        action: () => _$categoriesTable.draw(false)
      }
    ],
    responsive: { details: { type: 'column' } },
    columnDefs: [
      { targets: 0, className: 'control', defaultContent: '' },
      { targets: 1, data: 'name', title: l('Name'), sortable: false },
      {
        targets: 2,
        data: null,
        sortable: false,
        title: l('Actions'),
        render: function (data, type, row) {
          return `
                        <div class="d-flex justify-content-around">

                            <button type="button"
                                class="btn btn-sm btn-secondary edit-category"
                                data-id="${row.id}" title="${l('Edit')}">
                                <i class="fas fa-edit"></i>
                            </button>

                            <button type="button"
                                class="btn btn-sm btn-danger delete-category"
                                data-id="${row.id}" data-name="${row.name}"
                                title="${l('Delete')}">
                                <i class="fas fa-times"></i>
                            </button>

                        </div>`;
        }
      }
    ]
  });

  // =================== CREATE ===================
  $(document).on('click', '.create-category', function () {
    abp.ajax({
      url: abp.appPath + 'Admin/MobilePhoneCategories/CreateModal',
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#CategoryCreateModal div.modal-content').html(content);
        $('#CategoryCreateModal').modal('show');
      }
    });
  });

  abp.event.on('category.created', function () {
    _$categoriesTable.ajax.reload();
  });

  // =================== EDIT ===================
  $(document).on('click', '.edit-category', function () {
    const id = $(this).data('id');

    abp.ajax({
      url: abp.appPath + 'Admin/MobilePhoneCategories/EditModal?id=' + id,
      type: 'GET',
      dataType: 'html',
      success: function (content) {
        $('#CategoryEditModal div.modal-content').html(content);
        $('#CategoryEditModal').modal('show');
      }
    });
  });

  abp.event.on('category.edited', function () {
    _$categoriesTable.ajax.reload();
  });

  // =================== DELETE ===================
  $(document).on('click', '.delete-category', function () {
    const id = $(this).data('id');
    const name = $(this).data('name');

    abp.message.confirm(
      abp.utils.formatString(l("AreYouSureWantToDelete"), name),
      null,
      function (isConfirmed) {
        if (isConfirmed) {
          _categoryService.delete({ id: id }).done(function () {
            abp.notify.info(l("SuccessfullyDeleted"));
            _$categoriesTable.ajax.reload();
          });
        }
      }
    );
  });

  // =================== SEARCH ===================
  $('#CategoriesSearchForm .btn-search').on('click', function () {
    _$categoriesTable.ajax.reload();
  });

  $('#CategoriesSearchForm .txt-search').on('keypress', function (e) {
    if (e.which === 13) {
      _$categoriesTable.ajax.reload();
      return false;
    }
  });

  $('#CategoriesSearchForm .btn-clear').on('click', function () {
    $('#CategoriesSearchForm')[0].reset();
    _$categoriesTable.ajax.reload();
  });

})(jQuery);
