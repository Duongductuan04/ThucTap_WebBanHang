(function ($) {
    const l = abp.localization.getSource('SimpleTaskApp');
    const _$modal = $('#ImportEditModal');
    const _$form = _$modal.find('form');
    const _importService = abp.services.app.import;

    // ================== Lưu (Cập nhật) phiếu nhập ==================
    function saveImport() {
        if (!_$form.valid()) return;

        const importData = _$form.serializeFormToObject();
        importData.ImportDetails = [];

        // Duyệt qua các dòng chi tiết sản phẩm
        _$form.find('.import-detail-row').each(function () {
            const $row = $(this);
            const mobilePhoneId = parseInt($row.find('[name$=".MobilePhoneId"]').val());
            const quantity = parseInt($row.find('[name$=".Quantity"]').val());
            const importPrice = parseFloat($row.find('[name$=".ImportPrice"]').val());

            if (mobilePhoneId && quantity > 0 && importPrice >= 0) {
                importData.ImportDetails.push({
                    MobilePhoneId: mobilePhoneId,
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

        // Gọi API update thay vì create
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
    function createDetailRow(selectedId = "", quantity = "", importPrice = "") {
        let optionsHtml = `<option value="">-- ${l("Select")} --</option>`;
        window.availablePhones.forEach(p => {
            const selectedAttr = p.id == selectedId ? "selected" : "";
            optionsHtml += `<option value="${p.id}" ${selectedAttr}>${p.name}</option>`;
        });

        const newRow = `
            <div class="import-detail-row d-flex gap-2 mb-2">
                <select name="MobilePhoneId" class="form-control" required>${optionsHtml}</select>
                <input type="number" name="Quantity" class="form-control" placeholder="${l("Quantity")}" 
                       value="${quantity}" min="1" required />
                <input type="number" name="ImportPrice" class="form-control" placeholder="${l("ImportPrice")}" 
                       value="${importPrice}" min="0" step="0.01" required />
                <button class="btn btn-danger remove-detail" type="button">X</button>
            </div>
        `;

        _$form.find('.import-detail-container').append(newRow);
    }

    // ================== Gán sự kiện ==================

    // Thêm dòng chi tiết mới
    $(document).on('click', '.add-detail', function () {
        createDetailRow();
    });

    // Xóa dòng chi tiết
    $(document).on('click', '.remove-detail', function () {
        $(this).closest('.import-detail-row').remove();
    });

    // Nút Lưu
    _$form.closest('div.modal-content').find(".save-button").on('click', function (e) {
        e.preventDefault();
        saveImport();
    });

    // Enter để lưu
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
