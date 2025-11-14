(function ($) {
  const l = abp.localization.getSource('SimpleTaskApp');
  const _$modal = $('#ImportEditModal');
  const _$form = _$modal.find('form');
  const _importService = abp.services.app.import;

  // ================== Khởi tạo index cho các dòng chi tiết ==================
  let detailIndex = _$form.find('.import-detail-row').length;

  // ================== Lưu (Cập nhật) phiếu nhập ==================
  function saveImport() {
    if (!_$form.valid()) return;

    const importData = _$form.serializeFormToObject();
    importData.ImportDetails = [];

    _$form.find('.import-detail-row').each(function () {
      const $row = $(this);
      const mobilePhoneId = parseInt($row.find('select[name*="MobilePhoneId"]').val());
      const mobilePhoneColorId = parseInt($row.find('select[name*="MobilePhoneColorId"]').val()) || null;
      const quantity = parseInt($row.find('input[name*="Quantity"]').val());
      const importPrice = parseFloat($row.find('input[name*="ImportPrice"]').val());
      const detailId = parseInt($row.find('input[name*=".Id"]').val()) || 0;

      if (mobilePhoneId && quantity > 0 && importPrice >= 0) {
        importData.ImportDetails.push({
          Id: detailId,
          MobilePhoneId: mobilePhoneId,
          MobilePhoneColorId: mobilePhoneColorId, // ✅ thêm màu
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

    _importService.update(importData)
      .done(() => {
        _$modal.modal('hide');
        abp.notify.info(l('SavedSuccessfully'));
        abp.event.trigger('import.edited', importData);
        location.reload();
      })
      .always(() => {
        abp.ui.clearBusy(_$form);
      });
  }

  // ================== Tạo dòng chi tiết mới ==================
  function createDetailRow(selectedId = "", colorId = "", quantity = "", importPrice = "") {
    let phoneOptions = `<option value="">-- ${l("Select")} --</option>`;
    window.availablePhones.forEach(p => {
      const selectedAttr = p.id == selectedId ? "selected" : "";
      phoneOptions += `<option value="${p.id}" ${selectedAttr}>${p.name}</option>`;
    });

    const newRow = `
        <div class="import-detail-row d-flex gap-2 mb-2 align-items-center">
            <input type="hidden" name="ImportDetails[${detailIndex}].Id" value="0" />

            <select name="ImportDetails[${detailIndex}].MobilePhoneId" 
                    class="form-control phone-select" required>
                ${phoneOptions}
            </select>

            <select name="ImportDetails[${detailIndex}].MobilePhoneColorId" 
                    class="form-control color-select" ${selectedId ? "" : "disabled"}>
                <option value="">-- ${l("Color")} --</option>
            </select>

            <input type="number" name="ImportDetails[${detailIndex}].Quantity" 
                   class="form-control" placeholder="${l("Quantity")}" 
                   value="${quantity}" min="1" required />

            <input type="number" name="ImportDetails[${detailIndex}].ImportPrice" 
                   class="form-control" placeholder="${l("ImportPrice")}" 
                   value="${importPrice}" min="0" step="0.01" required />

            <button class="btn btn-danger remove-detail" type="button">X</button>
        </div>
        `;

    _$form.find('.import-detail-container').append(newRow);

    // Nếu có selectedId → tự load màu
    if (selectedId) {
      const $newRow = _$form.find('.import-detail-row').last();
      loadColorsForPhone($newRow.find('.phone-select'), colorId);
    }

    detailIndex++;
  }

  // ================== Load màu khi chọn sản phẩm ==================
  function loadColorsForPhone($phoneSelect, selectedColorId = null) {
    const $row = $phoneSelect.closest('.import-detail-row');
    const $colorSelect = $row.find('.color-select');
    const phoneId = $phoneSelect.val();

    $colorSelect.html(`<option value="">-- ${l("Color")} --</option>`);

    if (phoneId) {
      const colors = window.availableColors.filter(c => c.mobilePhoneId == phoneId);
      if (colors.length > 0) {
        colors.forEach(c => {
          const selected = c.id == selectedColorId ? "selected" : "";
          $colorSelect.append(`<option value="${c.id}" ${selected}>${c.colorName}</option>`);
        });
        $colorSelect.prop('disabled', false);
      } else {
        $colorSelect.prop('disabled', true);
      }
    } else {
      $colorSelect.prop('disabled', true);
    }
  }

  // ================== Gán sự kiện ==================
  $(document).on('click', '.add-detail', function () {
    createDetailRow();
  });

  $(document).on('click', '.remove-detail', function () {
    $(this).closest('.import-detail-row').remove();
  });

  $(document).on('change', '.phone-select', function () {
    loadColorsForPhone($(this));
  });

  _$form.closest('div.modal-content').find(".save-button").on('click', function (e) {
    e.preventDefault();
    saveImport();
  });

  _$form.on('keypress', 'input', function (e) {
    if (e.which === 13) {
      e.preventDefault();
      saveImport();
    }
  });

  _$modal.on('shown.bs.modal', function () {
    $.validator.unobtrusive.parse(_$form);
    _$form.find('input[type=text]:first').focus();
  });

})(jQuery);
