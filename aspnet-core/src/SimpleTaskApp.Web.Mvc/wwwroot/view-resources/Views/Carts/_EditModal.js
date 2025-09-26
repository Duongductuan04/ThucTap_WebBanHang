(function ($) {
    var l = abp.localization.getSource('SimpleTaskApp'),
        _$modal = $('#CartEditModal'),
        _$form = _$modal.find('form');

    var _cartService = abp.services.app.cart;

    function save() {
        if (!_$form.valid()) {
            return;
        }

        var cart = _$form.serializeFormToObject();
        abp.ui.setBusy(_$form);

        _cartService.update(cart).done(function () {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('cart.edited', cart);
            location.reload();
        }).always(function () {
            abp.ui.clearBusy(_$form);
        });
    }

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
