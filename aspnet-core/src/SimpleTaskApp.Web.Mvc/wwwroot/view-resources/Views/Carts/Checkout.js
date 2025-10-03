const selectAll = document.getElementById('selectAll');
const items = document.querySelectorAll('.selectItem');
const checkoutBtn = document.getElementById('checkoutBtn');
const totalAmountElement = document.getElementById('totalAmount');

// Hàm cập nhật tổng tiền
function updateTotalAmount() {
    let total = 0;

    items.forEach(item => {
        if (item.checked) {
            const row = item.closest('tr');
            const priceText = row.querySelector('td:nth-child(4)').textContent;
            const quantity = parseInt(row.querySelector('.quantity-input').value) || 1;
            const price = parseFloat(priceText.replace(/[^0-9]/g, '')) || 0;

            total += price * quantity;
        }
    });

    if (totalAmountElement) {
        totalAmountElement.textContent = formatCurrency(total);
    }
}

// Hàm định dạng tiền tệ
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Xử lý khi click nút Thanh toán
function handleCheckout(e) {
    e.preventDefault();

    const selectedIds = Array.from(items)
        .filter(i => i.checked)
        .map(i => i.value);

    if (selectedIds.length === 0) {
        alert('Vui lòng chọn ít nhất một sản phẩm để thanh toán.');
        return;
    }

    const url = new URL(checkoutUrl, window.location.origin);
    selectedIds.forEach(id => url.searchParams.append('cartIds', id));
    window.location.href = url.toString();
}

// Chọn tất cả
if (selectAll) {
    selectAll.addEventListener('change', function () {
        items.forEach(item => item.checked = selectAll.checked);
        updateTotalAmount();
    });
}

// Thay đổi lựa chọn sản phẩm hoặc số lượng
items.forEach(item => {
    item.addEventListener('change', () => {
        selectAll.checked = Array.from(items).every(i => i.checked);
        updateTotalAmount();
    });

    const quantityInput = item.closest('tr').querySelector('input[name="quantity"]');
    if (quantityInput) {
        quantityInput.addEventListener('input', updateTotalAmount);
        quantityInput.addEventListener('change', updateTotalAmount);
    }
});

// Gắn sự kiện click vào nút Thanh toán
if (checkoutBtn) {
    checkoutBtn.addEventListener('click', handleCheckout);
} $(function () {
    const $subtotal = $('#subtotal');
    const $shippingFee = $('#shippingFee');
    const $discountAmount = $('#discountAmount');
    const $finalAmount = $('#finalAmount');
    const $shippingSelect = $('#ShippingMethod');
    const $discountMessage = $('#discountMessage');
    const $discountCodeInput = $('#DiscountCode');
    const $discountId = $('#DiscountId');
    const $discountRow = $('#discountRow');

    function formatCurrency(num) {
        return num.toLocaleString('vi-VN') + ' đ';
    }

    function updateFinalAmount() {
        let subtotal = parseFloat($subtotal.data('value') || 0);
        let shippingFee = parseInt($shippingFee.data('value') || 0);
        let discount = parseInt($discountAmount.data('value') || 0);
        $finalAmount.text(formatCurrency(subtotal + shippingFee - discount));
        $finalAmount.data('value', subtotal + shippingFee - discount);
    }

    function updateShippingFee() {
        let fee = 20000; // tiêu chuẩn
        const val = $shippingSelect.val();
        if (val === '1') fee = 40000;
        if (val === '2') fee = 60000;
        $shippingFee.text(formatCurrency(fee));
        $shippingFee.data('value', fee);
        updateFinalAmount();
    }

    $shippingSelect.on('change', updateShippingFee);

    // Reset discount (chỉ input và thông tin giảm giá, không reset radio)
    function resetDiscountInput() {
        $discountAmount.text('0 đ').data('value', 0);
        $discountId.val('');
        $discountMessage.text('').removeClass('text-danger text-success');
        $discountRow.hide();
        updateFinalAmount();
    }

    // Reset hoàn toàn (input + radio)
    function resetDiscountAll() {
        $discountCodeInput.val('');
        $('input[name="selectedDiscount"]').prop('checked', false);
        resetDiscountInput();
    }

    // Khi nhập mã giảm giá, xóa thông báo cũ nếu rỗng
    $discountCodeInput.on('input', function () {
        if ($(this).val().trim() === '') resetDiscountInput();
    });

    // Áp dụng mã giảm giá
    $('#applyDiscountBtn').click(function () {
        resetDiscountInput(); // xóa thông báo cũ
        var selectedVoucher = $('input[name="selectedDiscount"]:checked').data('discount-code');
        var discountCode = selectedVoucher || $discountCodeInput.val().trim();

        if (!discountCode) {
            $discountMessage.text("Vui lòng chọn hoặc nhập mã giảm giá")
                .removeClass('text-success').addClass('text-danger');
            return;
        }

        if (selectedVoucher) {
            $discountCodeInput.val(selectedVoucher);
        }

        var orderItems = [];
        $('.cart-item').each(function () {
            var productId = $(this).data('product-id');
            var quantity = parseInt($(this).find('.badge').text()) || 0;
            var unitPrice = parseFloat($(this).data('unit-price')) || 0;
            orderItems.push({ productId, quantity, unitPrice });
        });

        abp.ajax({
            url: abp.appPath + 'api/services/app/Order/ApplyDiscount',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                DiscountCode: discountCode,
                TotalAmount: parseFloat($subtotal.data('value') || 0),
                OrderItems: orderItems
            }),
            success: function (result) {
                if (result && result.discountAmount > 0) {
                    $discountRow.show();
                    $discountAmount.text(formatCurrency(result.discountAmount));
                    $discountAmount.data('value', result.discountAmount);
                    $discountId.val(result.discountId);
                    $discountCodeInput.val(result.discountCode);
                    $discountMessage.text('Áp dụng mã giảm giá thành công!')
                        .removeClass('text-danger').addClass('text-success');
                } else {
                    resetDiscountInput();
                }
                updateFinalAmount();
            },
            error: function () {
                resetDiscountInput();
            }
        });
    });

    // Nút Hủy voucher
    $('#clearDiscountBtn').click(function () {
        resetDiscountAll();
    });

    updateShippingFee();
});
$(document).ready(function () {
    // Mở modal khi nhấn nút "Chọn Voucher"
    $('#btnChooseVoucher').on('click', function () {
        $('#voucherModal').modal('show');
    });

    // Xử lý khi chọn một voucher trong modal
    $(document).on('change', '.discount-radio', function () {
        // Lấy mã voucher từ thuộc tính data-
        let selectedDiscountCode = $(this).data('discount-code');

        // Điền mã vào ô input
        $('#DiscountCode').val(selectedDiscountCode);

        // Đóng modal
        $('#voucherModal').modal('hide');

        // Tự động kích hoạt sự kiện áp dụng voucher
        $('#applyDiscountBtn').trigger('click');
    });
});