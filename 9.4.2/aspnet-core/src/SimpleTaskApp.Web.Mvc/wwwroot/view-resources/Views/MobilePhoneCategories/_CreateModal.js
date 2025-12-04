(function ($) {
  const l = abp.localization.getSource('SimpleTaskApp');
  const _$modal = $('#CategoryCreateModal');
  const _$form = _$modal.find('form');
  const _categoryService = abp.services.app.mobilePhoneCategory;

  // Hàm lưu category
  function save() {
    // Kiểm tra validate
    if (!_$form.valid()) return;

    const dto = _$form.serializeFormToObject();

    abp.ui.setBusy(_$form);

    _categoryService.create(dto)
      .done(() => {
        _$modal.modal('hide');
        abp.notify.info(l("SavedSuccessfully"));
        abp.event.trigger("category.created");
      })
      .always(() => {
        abp.ui.clearBusy(_$form);
      });
  }

  // Click nút Save
  _$modal.find(".save-button").on("click", function (e) {
    e.preventDefault();
    save();
  });

  // Nhấn Enter trong input cũng lưu
  _$form.on('keypress', 'input', function (e) {
    if (e.which === 13) {
      e.preventDefault();
      save();
    }
  });

  // Khi modal hiển thị, parse form để validation chạy
  _$modal.on('shown.bs.modal', function () {
    $.validator.unobtrusive.parse(_$form);
    _$form.find('input[type=text]:first').focus();
  });

})(jQuery);
