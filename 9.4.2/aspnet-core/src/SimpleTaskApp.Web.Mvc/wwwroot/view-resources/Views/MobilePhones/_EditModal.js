(function ($) {
    var l = abp.localization.getSource('SimpleTaskApp'),
        _$modal = $('#MobilePhoneEditModal'),
        _$form = _$modal.find('form');

    var _mobilePhoneService = abp.services.app.mobilePhone;

    // === Hàm upload file ===
    function uploadFile(fileInput) {
        var file = fileInput[0]?.files[0];
        var dfd = $.Deferred();

        if (!file) {
            dfd.resolve({ result: { fileUrl: '' } });
            return dfd.promise();
        }

        var formData = new FormData();
        formData.append('file', file);

        $.ajax({
            url: '/Admin/MobilePhones/UploadImage',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                dfd.resolve(response);
            },
            error: function (xhr, status, error) {
                dfd.reject(error);
            }
        });

        return dfd.promise();
    }

    // === Lưu thông tin điện thoại + màu ===
    async function save() {
        if (!_$form.valid()) return;

        abp.ui.setBusy(_$form);

        try {
            var mobilePhone = _$form.serializeFormToObject();
            var $fileInput = _$form.find('input[name=ImageFile]');

            // Upload ảnh chính
            var response = await uploadFile($fileInput);
            if (response.result && response.result.fileUrl) {
                mobilePhone.ImageUrl = response.result.fileUrl;
            }

            // === Lấy danh sách màu ===
            var colors = [];
            var colorItems = $('#editColorList .color-item');

            for (let i = 0; i < colorItems.length; i++) {
                let $item = $(colorItems[i]);
                let colorId = parseInt($item.find('input[name=ColorId]').val()) || 0;
                let colorName = $item.find('input[name=ColorName]').val() || '';
                let colorHex = $item.find('input[name=ColorHex]').val() || '';
                let $colorFile = $item.find('input[name=ColorImage]');
                let colorImageUrl = $item.find('input[name=ExistingImageUrl]').val() || '';

                // Nếu có file mới upload, ghi đè ExistingImageUrl
                if ($colorFile[0].files.length > 0) {
                    let uploadResult = await uploadFile($colorFile);
                    colorImageUrl = uploadResult.result?.fileUrl || colorImageUrl;
                }

                if (colorName.trim()) {
                    colors.push({
                        Id: colorId,
                        ColorName: colorName,
                        ColorHex: colorHex,
                        ImageUrl: colorImageUrl
                    });
                }
            }

            mobilePhone.Colors = colors;

            // === Gọi API cập nhật ===
            _mobilePhoneService.update(mobilePhone).done(function () {
                _$modal.modal('hide');
                abp.notify.info(l('SavedSuccessfully'));
                abp.event.trigger('mobilePhone.edited', mobilePhone);
                location.reload();
            }).always(function () {
                abp.ui.clearBusy(_$form);
            });

        } catch (e) {
            console.error(e);
            abp.notify.error(l('FileUploadFailed'));
            abp.ui.clearBusy(_$form);
        }
    }

    // === Event lưu ===
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

    // === Thêm màu mới ===
    $('#addEditColor').click(function () {
        $('#editColorList').append(`
            <div class="color-item mt-2">
                <input type="hidden" name="ColorId" value="0">
                <input type="hidden" name="ExistingImageUrl" value="">
                <input type="text" name="ColorName" placeholder="Tên màu" class="form-control d-inline-block" style="width:30%">
                <input type="text" name="ColorHex" placeholder="#HEX" class="form-control d-inline-block" style="width:20%">
                <input type="file" name="ColorImage" accept="image/*" class="form-control d-inline-block" style="width:40%">
                <button type="button" class="btn btn-danger btn-sm remove-color">X</button>
            </div>
        `);
    });

    // === Xoá màu ===
    $(document).on('click', '.remove-color', function () {
        $(this).closest('.color-item').remove();
    });

})(jQuery);
