(function ($) {
    var l = abp.localization.getSource('SimpleTaskApp'),
        _$modal = $('#MobilePhoneCreateModal'),
        _$form = _$modal.find('form');

    var _mobilePhoneService = abp.services.app.mobilePhone;

    // Hàm upload file
    function uploadFile(fileInput) {
        var file = fileInput[0].files[0];
        if (!file) {
            return Promise.resolve({ result: { fileUrl: '' } });
        }

        var formData = new FormData();
        formData.append('file', file);

        return $.ajax({
            url: '/Admin/MobilePhones/UploadImage',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false
        });
    }

    async function save() {
        if (!_$form.valid()) {
            return;
        }

        var mobilePhone = _$form.serializeFormToObject();
        var $fileInput = _$form.find('input[name=ImageFile]');
        abp.ui.setBusy(_$form);

        try {
            // === Upload ảnh chính ===
            var response = await uploadFile($fileInput);
            mobilePhone.ImageUrl = response.result?.fileUrl || '';

            // === Lấy danh sách màu ===
            var colors = [];
            var colorItems = $('#colorList .color-item');

            for (let i = 0; i < colorItems.length; i++) {
                let $item = $(colorItems[i]);
                let colorName = $item.find('input[name=ColorName]').val();
                let colorHex = $item.find('input[name=ColorHex]').val();
                let $colorFile = $item.find('input[name=ColorImage]');
                let colorImageUrl = '';

                if ($colorFile[0].files.length > 0) {
                    let uploadResult = await uploadFile($colorFile);
                    colorImageUrl = uploadResult.result?.fileUrl || '';
                }

                if (colorName) {
                    colors.push({
                        ColorName: colorName,
                        ColorHex: colorHex,
                        ImageUrl: colorImageUrl
                    });
                }
            }

            mobilePhone.Colors = colors;

            // === Gửi về backend ===
            _mobilePhoneService.create(mobilePhone).done(function () {
                _$modal.modal('hide');
                abp.notify.info(l('SavedSuccessfully'));
                abp.event.trigger('mobilePhone.created', mobilePhone);
                location.reload();
            }).always(function () {
                abp.ui.clearBusy(_$form);
            });
        }
        catch (e) {
            abp.notify.error(l('FileUploadFailed'));
            abp.ui.clearBusy(_$form);
        }
    }

    // Event click & Enter
    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        save();
    });

    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            save();
        }
    });

    _$modal.on('shown.bs.modal', function () {
        $.validator.unobtrusive.parse(_$form);
        _$form.find('input[type=text]:first').focus();
    });

    // ✅ Thêm nút thêm màu
    $('#addColor').click(function () {
        $('#colorList').append(`
            <div class="color-item mt-2">
                <input type="text" name="ColorName" placeholder="Tên màu" class="form-control d-inline-block" style="width:30%">
                <input type="text" name="ColorHex" placeholder="#HEX" class="form-control d-inline-block" style="width:20%">
                <input type="file" name="ColorImage" class="form-control d-inline-block" style="width:40%">
                <button type="button" class="btn btn-danger btn-sm remove-color">X</button>
            </div>
        `);
    });

    $(document).on('click', '.remove-color', function () {
        $(this).closest('.color-item').remove();
    });

})(jQuery);
