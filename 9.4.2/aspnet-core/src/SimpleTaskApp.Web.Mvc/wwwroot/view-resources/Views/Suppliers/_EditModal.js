(function ($) {

  const l = abp.localization.getSource('SimpleTaskApp');
  const _$modal = $('#SupplierEditModal'); // Modal của Supplier
  const _$form = _$modal.find('form');
  const _supplierService = abp.services.app.supplier; // Service Supplier

  // ⭐ Hàm Save (Update)
  function save() {
    if (!_$form.valid()) return;

    const dto = _$form.serializeFormToObject(); // Lấy Id + các trường Supplier

    abp.ui.setBusy(_$form);

    _supplierService.update(dto)
      .done(() => {
        _$modal.modal('hide');
        abp.notify.info(l("SavedSuccessfully"));
        abp.event.trigger("supplier.edited"); // trigger event
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
