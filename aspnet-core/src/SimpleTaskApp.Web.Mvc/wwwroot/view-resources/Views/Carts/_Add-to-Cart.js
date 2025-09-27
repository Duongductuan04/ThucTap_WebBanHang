$(function () {
    var _cartService = abp.services.app.cart;

    $('#addToCartForm').submit(function (e) {
        e.preventDefault();
        var $form = $(this);
        var token = $form.find('input[name="__RequestVerificationToken"]').val();
        var mobilePhoneId = parseInt($form.find('input[name="mobilePhoneId"]').val());
        var quantity = parseInt($form.find('input[name="quantity"]').val()) || 1;

        abp.ui.setBusy($form);

        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: {
                __RequestVerificationToken: token,
                mobilePhoneId: mobilePhoneId,
                quantity: quantity
            },
            success: function (response) {
                if (response.success) {
                    abp.notify.success(response.message || 'Thêm vào giỏ hàng thành công.');
                    setTimeout(function () {
                        location.reload(); // reload sau 500ms
                    }, 500);
                    abp.event.trigger('cart.updated');

                } else {
                    abp.notify.error(response.message || 'Thêm vào giỏ hàng thất bại.');
                }
            },
            error: function () {
                abp.notify.error('Có lỗi xảy ra.');
            },
            complete: function () {
                abp.ui.clearBusy($form);
            }
        });
    });
});
