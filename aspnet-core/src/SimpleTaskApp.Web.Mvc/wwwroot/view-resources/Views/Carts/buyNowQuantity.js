$(function () {

    // ================== Hàm tính tổng số lượng ==================
    function getTotalQuantity() {
        let totalQty = 0;
        $("input[name='quantity']").each(function () {
            let qty = parseInt($(this).val()) || 1;
            const maxStock = parseInt($(this).attr('max')) || qty;

            if (qty < 1) qty = 1;
            if (qty > maxStock) {
                alert("Số lượng vượt quá tồn kho! Tối đa: " + maxStock);
                $(this).val(maxStock);
                qty = maxStock;
            }

            const checkbox = $(this).closest("tr").find("input.selectItem");
            if (checkbox.length === 0 || checkbox.is(":checked")) {
                totalQty += qty;
            }
        });
        return totalQty;
    }

    // ================== Cập nhật tổng tiền ==================
    function updateCartTotals() {
        let grandTotal = 0, selectedTotal = 0;

        $("tr[data-cart-id]").each(function () {
            const row = $(this);
            const quantity = parseInt(row.find(".cartQuantity").val()) || 1;
            const price = parseFloat(row.find(".cartQuantity").data("price")) || 0;
            const total = price * quantity;

            row.find(".item-total").text(total.toLocaleString('vi-VN') + " VNĐ");
            grandTotal += total;

            const checkbox = row.find("input.selectItem");
            if (checkbox.length === 0 || checkbox.is(":checked")) {
                selectedTotal += total;
            }
        });

        $("#grandTotal").text(grandTotal.toLocaleString('vi-VN') + " VNĐ");
        $("#selectedTotal").text(selectedTotal.toLocaleString('vi-VN') + " VNĐ");
    }

    // ================== Cập nhật số lượng AJAX ==================
    function updateCartQuantity(cartId, quantity) {
        $.ajax({
            url: updateQuantityUrl,
            type: 'POST',
            data: {
                cartId: cartId,
                quantity: quantity,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                if (res.success) {
                    const row = $('tr[data-cart-id="' + cartId + '"]');
                    const price = parseFloat(row.find('.cartQuantity').data('price')) || 0;
                    const total = price * quantity;
                    row.find('.item-total').text(total.toLocaleString('vi-VN') + ' VNĐ');
                    updateCartTotals();
                } else {
                    alert(res.message);
                }
            },
            error: function () {
                alert("Có lỗi xảy ra khi cập nhật số lượng!");
            }
        });
    }

    // ================== Xử lý nút tăng/giảm ==================
    $(".btn-qty.increase, .btn-qty.decrease").click(function () {
        const input = $(this).siblings('.cartQuantity, #buyNowQuantity');
        let val = parseInt(input.val()) || 1;
        const max = parseInt(input.attr("max")) || 100;
        const min = parseInt(input.attr("min")) || 1;

        if ($(this).hasClass('increase') && val < max) val++;
        if ($(this).hasClass('decrease') && val > min) val--;

        input.val(val);

        const cartId = input.data("cart-id");
        if (cartId) updateCartQuantity(cartId, val);

        updateCartTotals();
    });

  
    // ================== Checkbox ==================
    $("input.selectItem").change(updateCartTotals);

    $("#selectAll").change(function () {
        const checked = $(this).is(":checked");
        $("input.selectItem").prop("checked", checked);
        updateCartTotals();
    });


    // ================== Submit Mua Ngay ==================
    $("#buyNowForm").submit(function () {
        const qty = getTotalQuantity();
        $("#buyNowQuantity").val(qty);
        if (qty <= 0) {
            alert("Bạn chưa chọn sản phẩm nào!");
            return false;
        }
    });

    // ================== Khởi tạo tổng tiền khi load ==================
    updateCartTotals();
});
