(function ($) {

  const l = abp.localization.getSource('SimpleTaskApp');
  const _$modal = $('#CategoryEditModal');
  const _$form = _$modal.find('form');
  const _categoryService = abp.services.app.mobilePhoneCategory;

  // ⭐ Hàm Save (Update)
  function save() {
    if (!_$form.valid()) return;

    const dto = _$form.serializeFormToObject(); // Lấy Id + Name

    abp.ui.setBusy(_$form);

    _categoryService.update(dto)
      .done(() => {
        _$modal.modal('hide');
        abp.notify.info(l("SavedSuccessfully"));
        abp.event.trigger("category.edited");
      })
      .always(() => {
        abp.ui.clearBusy(_$form);
      });
  }

  // ⭐ Gắn nút Save
  _$modal.find(".save-button").on("click", function (e) {
    e.preventDefault();
    save();
  });

})(jQuery);
