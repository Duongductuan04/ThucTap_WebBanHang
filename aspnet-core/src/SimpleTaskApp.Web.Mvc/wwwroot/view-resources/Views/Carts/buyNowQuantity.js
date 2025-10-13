$(function () {
    const modal = new bootstrap.Modal(document.getElementById('quantityModal'));
    const modalQuantity = $('#modalQuantity');
    let currentAction = '';
    const stockQuantity = parseInt(modalQuantity.attr('max')) || 100;


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

    // ================== Xử lý tăng/giảm số lượng ==================
    $(".btn-qty.increase, .btn-qty.decrease").click(function () {
        const input = $(this).siblings(' #buyNowQuantity, #modalQuantity');
        let val = parseInt(input.val()) || 1;
        const max = parseInt(input.attr("max")) || 100;
        const min = parseInt(input.attr("min")) || 1;

        if ($(this).hasClass("increase") && val < max) val++;
        if ($(this).hasClass("decrease") && val > min) val--;

        input.val(val);

        const cartId = input.data("cart-id");
        if (cartId) updateCartQuantity(cartId, val);

        updateCartTotals();
    });
 

    // ================== Mua Ngay / Thêm vào giỏ ==================
    $('.add-to-cart-btn, .buy-now-btn').click(function () {
        currentAction = $(this).data("action");
        modalQuantity.val(1);
        modal.show();
    });

    $('#confirmAction').click(function () {
        const quantity = parseInt(modalQuantity.val()) || 1;
        if (quantity < 1 || quantity > stockQuantity) {
            alert("Số lượng không hợp lệ!");
            return;
        }

        if (currentAction === "addToCart") {
            $('#cartQuantity').val(quantity);
            $('#addToCartForm').submit();
        } else if (currentAction === "buyNow") {
            $('#buyNowQuantity').val(quantity);
            $('#buyNowForm').submit();
        }

        modal.hide();
    });

    $('.btn-secondary').click(function () {
        modal.hide();
    });

  
});


$(function () {
    const $selectAll = $('#selectAll');
    const $items = $('.selectItem');
    const $selectedTotal = $('#selectedTotal');

    // Hàm tính tổng tiền đã chọn
    function updateSelectedTotal() {
        let total = 0;
        $items.each(function () {
            const $checkbox = $(this);
            const $row = $checkbox.closest('tr');
            const price = parseFloat($checkbox.data('price')) || 0;
            const quantity = parseInt($row.find('.cartQuantity').val()) || 1;

            if ($checkbox.is(':checked')) {
                total += price * quantity;
            }
        });

        $selectedTotal.text(new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(total));
    }

    // Checkbox "Chọn tất cả"
    $selectAll.on('change', function () {
        $items.prop('checked', this.checked);
        updateSelectedTotal();
    });

    // Checkbox từng sản phẩm
    $items.on('change', updateSelectedTotal);

    // Nút tăng giảm số lượng
    $(document).on('click', '.btn-qty', function () {
        const $btn = $(this);
        const $input = $btn.siblings('.cartQuantity');
        let val = parseInt($input.val()) || 1;
        const max = parseInt($input.attr('max')) || 100;
        const min = parseInt($input.attr('min')) || 1;

        if ($btn.hasClass('increase')) val = Math.min(val + 1, max);
        else if ($btn.hasClass('decrease')) val = Math.max(val - 1, min);

        $input.val(val);

        // Nếu checkbox sản phẩm đang chọn, cập nhật tổng tiền
        if ($input.closest('tr').find('.selectItem').is(':checked')) {
            updateSelectedTotal();
        }

        // Gọi ajax update số lượng lên server (nếu muốn)
        const cartId = $input.data('cart-id');
        let price = $input.data('price');

        // Nếu price là string có dấu ".", loại bỏ trước khi parse
        if (typeof price === 'string') {
            price = parseFloat(price.replace(/\./g, '')) || 0;
        } else {
            price = parseFloat(price) || 0;
        }

        $.post(updateQuantityUrl, { cartId, quantity: val }, function () {
            const total = val * price;

            // Cập nhật thành tiền cột tương ứng
            $input.closest('tr').find('.item-total').text(total.toLocaleString('vi-VN') + ' VNĐ');

            // Cập nhật tổng đã chọn
            updateSelectedTotal();
        });

    });

    // Khởi tạo
    updateSelectedTotal();
});
