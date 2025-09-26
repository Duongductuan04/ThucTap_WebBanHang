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

    function save() {
        if (!_$form.valid()) {
            return;
        }

        var mobilePhone = _$form.serializeFormToObject();
        var $fileInput = _$form.find('input[type=file]');
        abp.ui.setBusy(_$form);

        uploadFile($fileInput).done(function (response) {
            // Lấy fileUrl từ response.result
            mobilePhone.ImageUrl = response.result.fileUrl;

            _mobilePhoneService.create(mobilePhone).done(function () {
                _$modal.modal('hide');
                abp.notify.info(l('SavedSuccessfully'));
                abp.event.trigger('mobilePhone.created', mobilePhone);
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
