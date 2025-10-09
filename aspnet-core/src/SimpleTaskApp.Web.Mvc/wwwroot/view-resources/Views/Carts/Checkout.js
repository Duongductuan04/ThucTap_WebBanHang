const selectAll = document.getElementById('selectAll');
const items = document.querySelectorAll('.selectItem');
const checkoutBtn = document.getElementById('checkoutBtn');
const totalAmountElement = document.getElementById('totalAmount');

// ----------------- Hàm chung -----------------
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Cập nhật tổng tiền dựa trên checkbox và số lượng
function updateTotalAmount() {
    let total = 0;
    items.forEach(item => {
        if (item.checked) {
            const row = item.closest('tr');
            const priceText = row.querySelector('td:nth-child(4)').textContent;
            const quantity = parseInt(row.querySelector('.quantity-input')?.value || 1);
            const price = parseFloat(priceText.replace(/[^0-9]/g, '')) || 0;
            total += price * quantity;
        }
    });
    if (totalAmountElement) {
        totalAmountElement.textContent = formatCurrency(total);
    }
}

// ----------------- Checkout -----------------
function handleCheckout(e) {
    e.preventDefault();
    const selectedIds = Array.from(items).filter(i => i.checked).map(i => i.value);
    if (selectedIds.length === 0) {
        alert('Vui lòng chọn ít nhất một sản phẩm để thanh toán.');
        return;
    }
    const url = new URL(checkoutUrl, window.location.origin);
    selectedIds.forEach(id => url.searchParams.append('cartIds', id));
    window.location.href = url.toString();
}

// Chọn tất cả checkbox
if (selectAll) {
    selectAll.addEventListener('change', () => {
        items.forEach(item => item.checked = selectAll.checked);
        updateTotalAmount();
    });
}

// Checkbox từng sản phẩm + input số lượng
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

// Nút checkout
if (checkoutBtn) {
    checkoutBtn.addEventListener('click', handleCheckout);
}

// ----------------- Voucher & Shipping -----------------
$(function () {
    const $subtotal = $('#subtotal');
    const $shippingFee = $('#shippingFee');
    const $discountAmount = $('#discountAmount');
    const $finalAmount = $('#finalAmount');
    const $shippingSelect = $('#ShippingMethod');
    const $discountMessage = $('#discountMessage');
    const $discountCodeInput = $('#DiscountCode');
    const $discountId = $('#DiscountId');
    const $discountRow = $('#discountRow');

    // Cập nhật tổng tiền cuối
    function updateFinalAmount() {
        const subtotal = parseFloat($subtotal.data('value') || 0);
        const shippingFee = parseInt($shippingFee.data('value') || 0);
        const discount = parseInt($discountAmount.data('value') || 0);
        $finalAmount.text(formatCurrency(subtotal + shippingFee - discount));
        $finalAmount.data('value', subtotal + shippingFee - discount);
    }

    // Cập nhật phí vận chuyển
    function updateShippingFee() {
        let fee = 20000; // tiêu chuẩn
        const val = $shippingSelect.val();
        if (val === '1') fee = 40000;
        if (val === '2') fee = 60000;
        $shippingFee.text(formatCurrency(fee)).data('value', fee);
        updateFinalAmount();
    }

    $shippingSelect.on('change', updateShippingFee);

    // Reset discount input
    function resetDiscountInput() {
        $discountAmount.text('0 đ').data('value', 0);
        $discountId.val('');
        $discountMessage.text('').removeClass('text-danger text-success');
        $discountRow.hide();
        updateFinalAmount();
    }

    // Reset hoàn toàn
    function resetDiscountAll() {
        $discountCodeInput.val('');
        $('input[name="selectedDiscount"]').prop('checked', false);
        resetDiscountInput();
    }

    $discountCodeInput.on('input', function () {
        if ($(this).val().trim() === '') resetDiscountInput();
    });

    // Áp dụng voucher
    $('#applyDiscountBtn').click(function () {
        resetDiscountInput();
        const selectedVoucher = $('input[name="selectedDiscount"]:checked').data('discount-code');
        const discountCode = selectedVoucher || $discountCodeInput.val().trim();

        if (!discountCode) {
            $discountMessage.text("Vui lòng chọn hoặc nhập mã giảm giá")
                .removeClass('text-success')
                .addClass('text-danger');
            return;
        }

        if (selectedVoucher) $discountCodeInput.val(selectedVoucher);

        const orderItems = [];
        $('.cart-item').each(function () {
            orderItems.push({
                productId: $(this).data('product-id'),
                quantity: parseInt($(this).find('.badge').text()) || 0,
                unitPrice: parseFloat($(this).data('unit-price')) || 0
            });
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
                    $discountAmount.text(formatCurrency(result.discountAmount)).data('value', result.discountAmount);
                    $discountId.val(result.discountId);
                    $discountCodeInput.val(result.discountCode);
                    $discountMessage.text('Áp dụng mã giảm giá thành công!')
                        .removeClass('text-danger').addClass('text-success');
                } else {
                    resetDiscountInput();
                }
                updateFinalAmount();
            },
            error: resetDiscountInput
        });
    });

    // Nút Hủy voucher
    $('#clearDiscountBtn').click(resetDiscountAll);

    // Mở modal chọn voucher
    $('#btnChooseVoucher').on('click', function () {
        $('#voucherModal').modal('show');

        // Khi modal mở, tự động điền voucher đã chọn
        const selectedRadio = $('.discount-radio:checked');
        if (selectedRadio.length) {
            $discountCodeInput.val(selectedRadio.data('discount-code'));
        }
    });

    // Khi chọn voucher trong modal
    $(document).on('change', '.discount-radio', function () {
        const selectedDiscountCode = $(this).data('discount-code');
        $discountCodeInput.val(selectedDiscountCode);
        $('#voucherModal').modal('hide');
        $('#applyDiscountBtn').trigger('click');
    });

    updateShippingFee();
});
//Chọn địa chỉ 
$(document).ready(function () {
    var locations = [];

    // 1️⃣ Load JSON địa chỉ
    $.getJSON('/data/vietnam_provinces.json', function (data) {
        locations = data;
        var provinceSelect = $('#Province');
        locations.forEach(function (province) {
            provinceSelect.append('<option value="' + province.name + '">' + province.name + '</option>');
        });
    });

    // 2️⃣ Khi chọn Tỉnh → load Quận/Huyện
    $('#Province').change(function () {
        var selectedProvince = $(this).val();
        var districtSelect = $('#District');
        var wardSelect = $('#Ward');
        districtSelect.empty().append('<option value="">Chọn Quận/Huyện</option>');
        wardSelect.empty().append('<option value="">Chọn Xã/Phường</option>');

        var province = locations.find(p => p.name === selectedProvince);
        if (province) {
            province.districts.forEach(function (district) {
                districtSelect.append('<option value="' + district.name + '">' + district.name + '</option>');
            });
        }
    });

    // 3️⃣ Khi chọn Quận → load Xã/Phường
    $('#District').change(function () {
        var selectedProvince = $('#Province').val();
        var selectedDistrict = $(this).val();
        var wardSelect = $('#Ward');
        wardSelect.empty().append('<option value="">Chọn Xã/Phường</option>');

        var province = locations.find(p => p.name === selectedProvince);
        if (!province) return;

        var district = province.districts.find(d => d.name === selectedDistrict);
        if (!district) return;

        district.wards.forEach(function (ward) {
            // ward là object, lấy ward.name
            wardSelect.append('<option value="' + ward.name + '">' + ward.name + '</option>');
        });
    });

    // 4️⃣ Khi submit, nối full địa chỉ vào input RecipientAddress
    $('#btnPlaceOrder').click(function () {
        var detail = $('#DetailAddress').val().trim();
        var ward = $('#Ward').val();
        var district = $('#District').val();
        var province = $('#Province').val();

        var fullAddress = '';
        if (detail) fullAddress += detail;
        if (ward) fullAddress += ', ' + ward;
        if (district) fullAddress += ', ' + district;
        if (province) fullAddress += ', ' + province;

        $('#DetailAddress').val(fullAddress);
    });
});
