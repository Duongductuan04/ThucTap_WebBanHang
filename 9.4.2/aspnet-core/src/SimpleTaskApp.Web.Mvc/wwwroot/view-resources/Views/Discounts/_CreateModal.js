(function ($) {
    var l = abp.localization.getSource('SimpleTaskApp'),
        _$modal = $('#DiscountCreateModal'),
        _$form = _$modal.find('form');

    var _discountService = abp.services.app.discount;

    function save() {
        if (!_$form.valid()) {
            return;
        }

        // Lấy dữ liệu form thành object
        var discount = _$form.serializeFormToObject();

        // Lấy danh sách category/product đã chọn nếu ApplyType = 1 hoặc 2
        if (discount.ApplyType == 1) {
            discount.Categories = [];
            _$form.find('select[name="Categories"] option:selected').each(function () {
                discount.Categories.push({ CategoryId: parseInt($(this).val()) });
            });
        } else if (discount.ApplyType == 2) {
            discount.Products = [];
            _$form.find('select[name="Products"] option:selected').each(function () {
                discount.Products.push({ MobilePhoneId: parseInt($(this).val()) });
            });
        }

        abp.ui.setBusy(_$form);

        _discountService.create(discount).done(function () {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('discount.created', discount);
            location.reload();
        }).always(function () {
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

    // Hiển thị modal
    _$modal.on('shown.bs.modal', function () {
        $.validator.unobtrusive.parse(_$form);
        _$form.find('input[type=text]:first').focus();
    });

    // Hiển thị dropdown dựa trên ApplyType
    _$form.find('select[name="ApplyType"]').change(function () {
        var type = $(this).val();
        if (type == "0") {
            _$form.find('.category-select').hide();
            _$form.find('.product-select').hide();
        } else if (type == "1") {
            _$form.find('.category-select').show();
            _$form.find('.product-select').hide();
        } else if (type == "2") {
            _$form.find('.category-select').hide();
            _$form.find('.product-select').show();
        }
    }).trigger('change'); // trigger để cập nhật ngay khi mở modal
})(jQuery);
