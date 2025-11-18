(function ($) {
  const l = abp.localization.getSource('SimpleTaskApp');
  const _$modal = $('#ImportCreateModal');
  const _$form = _$modal.find('form');
  const _importService = abp.services.app.import;

  // ================= Lưu phiếu nhập =================
  function saveImport() {
    if (!_$form.valid()) return;

    const importData = _$form.serializeFormToObject();
    importData.ImportDetails = [];

    // Duyệt từng dòng chi tiết sản phẩm
    _$form.find('.import-detail-row').each(function () {
      const $row = $(this);
      const mobilePhoneId = parseInt($row.find('[name="MobilePhoneId"]').val());
      const mobilePhoneColorId = parseInt($row.find('[name="MobilePhoneColorId"]').val()) || null;
      const quantity = parseInt($row.find('[name="Quantity"]').val());
      const importPrice = parseFloat($row.find('[name="ImportPrice"]').val());

      if (mobilePhoneId && quantity > 0 && importPrice > 0) {
        importData.ImportDetails.push({
          MobilePhoneId: mobilePhoneId,
          MobilePhoneColorId: mobilePhoneColorId, // ✅ gửi thêm màu
          Quantity: quantity,
          ImportPrice: importPrice
        });
      }
    });

    if (importData.ImportDetails.length === 0) {
      abp.message.warn(l("AddAtLeastOneProduct"));
      return;
    }

    abp.ui.setBusy(_$form);

    _importService.create(importData)
      .done(() => {
        _$modal.modal('hide');
        abp.notify.info(l('SavedSuccessfully'));
        abp.event.trigger('import.created', importData);
        location.reload();
      })
      .always(() => {
        abp.ui.clearBusy(_$form);
      });
  }

  // ================= Tạo dòng chi tiết =================
  function createDetailRow() {
    let phoneOptions = `<option value="">-- ${l("Select")} --</option>`;
    window.availablePhones.forEach(p => {
      phoneOptions += `<option value="${p.id}">${p.name}</option>`;
    });

    const newRow = `
            <div class="import-detail-row d-flex gap-2 mb-2 align-items-center">
                <select name="MobilePhoneId" class="form-control phone-select" required>
                    ${phoneOptions}
                </select>

                <select name="MobilePhoneColorId" class="form-control color-select" disabled>
                    <option value="">-- ${l("Color")} --</option>
                </select>

                <input type="number" name="Quantity" class="form-control" placeholder="${l("Quantity")}" min="1" required />
                <input type="number" name="ImportPrice" class="form-control" placeholder="${l("ImportPrice")}" min="0" step="0.01" required />
                <button class="btn btn-danger remove-detail" type="button">X</button>
            </div>
        `;

    _$form.find('.import-detail-container').append(newRow);
  }

  // ================= Gán sự kiện =================

  // Khi chọn sản phẩm → load danh sách màu
  $(document).on('change', '.phone-select', function () {
    const $row = $(this).closest('.import-detail-row');
    const phoneId = $(this).val();
    const $colorSelect = $row.find('.color-select');

    $colorSelect.html(`<option value="">-- ${l("Color")} --</option>`);

    if (phoneId) {
      // Lấy màu của sản phẩm này từ biến window.availableColors (bạn cần truyền biến này từ backend ra view)
      const colors = window.availableColors.filter(c => c.mobilePhoneId == phoneId);
      if (colors.length > 0) {
        colors.forEach(c => {
          $colorSelect.append(`<option value="${c.id}">${c.colorName}</option>`);
        });
        $colorSelect.prop('disabled', false);
      } else {
        $colorSelect.prop('disabled', true);
      }
    } else {
      $colorSelect.prop('disabled', true);
    }
  });

  // Thêm dòng chi tiết sản phẩm
  $(document).on('click', '.add-detail', function () {
    createDetailRow();
  });

  // Xóa dòng chi tiết sản phẩm
  $(document).on('click', '.remove-detail', function () {
    $(this).closest('.import-detail-row').remove();
  });

  // Lưu phiếu nhập
  _$form.closest('div.modal-content').find(".save-button").on('click', function (e) {
    e.preventDefault();
    saveImport();
  });

  // Nhấn Enter trong input cũng thực hiện lưu
  _$form.on('keypress', 'input', function (e) {
    if (e.which === 13) {
      e.preventDefault();
      saveImport();
    }
  });

  // Khi modal hiển thị
  _$modal.on('shown.bs.modal', function () {
    $.validator.unobtrusive.parse(_$form);
    _$form.find('input[type=text]:first').focus();
  });

})(jQuery);
