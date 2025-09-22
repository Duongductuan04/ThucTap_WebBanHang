(function ($) {
    var l = abp.localization.getSource('SimpleTaskApp'),
        _$modal = $('#MobilePhoneEditModal'),
        _$form = _$modal.find('form');

    var _mobilePhoneService = abp.services.app.mobilePhone;

    // Hàm upload file - SỬA LẠI
    function uploadFile(fileInput) {
        var file = fileInput[0].files[0];
        var dfd = $.Deferred(); // Tạo jQuery Deferred object

        if (!file) {
            // Nếu không có file, resolve với object rỗng
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

    function save() {
        if (!_$form.valid()) {
            return;
        }

        var $fileInput = _$form.find('input[type=file]');
        abp.ui.setBusy(_$form);

        uploadFile($fileInput).done(function (response) {
            // Lấy dữ liệu form SAU KHI upload file hoàn tất
            var mobilePhone = _$form.serializeFormToObject();

            if (response.result && response.result.fileUrl) {
                mobilePhone.ImageUrl = response.result.fileUrl;
            }

            _mobilePhoneService.update(mobilePhone).done(function () {
                _$modal.modal('hide');
                abp.notify.info(l('SavedSuccessfully'));
                abp.event.trigger('mobilePhone.edited', mobilePhone);
                location.reload();
            }).always(function () {
                abp.ui.clearBusy(_$form);
            });

        }).fail(function () {
            abp.notify.error(l('FileUploadFailed'));
            abp.ui.clearBusy(_$form);
        });
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
})(jQuery);