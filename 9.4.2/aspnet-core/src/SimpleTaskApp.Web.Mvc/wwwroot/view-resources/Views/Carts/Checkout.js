const selectAll = document.getElementById('selectAll');
const items = document.querySelectorAll('.selectItem');
const checkoutBtn = document.getElementById('checkoutBtn');
const totalAmountElement = document.getElementById('totalAmount');

const formatCurrency = amount => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);

const updateTotalAmount = () => {
    let total = 0;
    items.forEach(item => {
        if (item.checked) {
            const row = item.closest('tr');
            const price = parseFloat(row.querySelector('td:nth-child(4)').textContent.replace(/[^0-9]/g, '')) || 0;
            const qty = parseInt(row.querySelector('.quantity-input')?.value || 1);
            total += price * qty;
        }
    });
    if (totalAmountElement) totalAmountElement.textContent = formatCurrency(total);
};

if (selectAll) selectAll.addEventListener('change', () => {
    items.forEach(i => i.checked = selectAll.checked);
    updateTotalAmount();
});

items.forEach(item => {
    item.addEventListener('change', () => {
        selectAll.checked = Array.from(items).every(i => i.checked);
        updateTotalAmount();
    });
    const qtyInput = item.closest('tr').querySelector('input[name="quantity"]');
    if (qtyInput) qtyInput.addEventListener('input', updateTotalAmount);
});

// Checkout
if (checkoutBtn) checkoutBtn.addEventListener('click', e => {
    e.preventDefault();
    const selectedIds = Array.from(items).filter(i => i.checked).map(i => i.value);
    if (!selectedIds.length) return alert('Vui lòng chọn ít nhất một sản phẩm.');
    const url = new URL(checkoutUrl, window.location.origin);
    selectedIds.forEach(id => url.searchParams.append('cartIds', id));
    window.location.href = url.toString();
});

// Voucher & Shipping
$(function () {
    const $subtotal = $('#subtotal'), $shippingFee = $('#shippingFee'), $discountAmount = $('#discountAmount'), $finalAmount = $('#finalAmount');
    const $shippingSelect = $('#ShippingMethod'), $discountMessage = $('#discountMessage');
    const $discountCodeInput = $('#DiscountCode'), $discountId = $('#DiscountId'), $discountRow = $('#discountRow');

    const updateFinalAmount = () => {
        const subtotal = parseFloat($subtotal.data('value') || 0);
        const shipping = parseInt($shippingFee.data('value') || 0);
        const discount = parseInt($discountAmount.data('value') || 0);
        $finalAmount.text(formatCurrency(subtotal + shipping - discount)).data('value', subtotal + shipping - discount);
    };

    const updateShippingFee = () => {
        let fee = { '0': 20000, '1': 40000, '2': 60000 }[$shippingSelect.val()] || 20000;
        $shippingFee.text(formatCurrency(fee)).data('value', fee);
      updateFinalAmount();
      $shippingSelect.on('change', updateShippingFee);

    };

    const resetDiscountInput = () => {
        $discountAmount.text('0 đ').data('value', 0);
        $discountId.val(''); $discountMessage.text('').removeClass('text-danger text-success'); $discountRow.hide();
        updateFinalAmount();
    };

    const resetDiscountAll = () => { $discountCodeInput.val(''); $('input[name="selectedDiscount"]').prop('checked', false); resetDiscountInput(); };

    $discountCodeInput.on('input', () => { if (!$discountCodeInput.val().trim()) resetDiscountInput(); });

    $('#applyDiscountBtn').click(() => {
        resetDiscountInput();
        const selectedVoucher = $('input[name="selectedDiscount"]:checked').data('discount-code');
        const discountCode = selectedVoucher || $discountCodeInput.val().trim();
        if (!discountCode) return $discountMessage.text("Vui lòng chọn hoặc nhập mã giảm giá").removeClass('text-success').addClass('text-danger');
        if (selectedVoucher) $discountCodeInput.val(selectedVoucher);

        const orderItems = $('.cart-item').map(function () {
            return { productId: $(this).data('product-id'), quantity: parseInt($(this).find('.badge').text()) || 0, unitPrice: parseFloat($(this).data('unit-price')) || 0 };
        }).get();

        abp.ajax({
            url: abp.appPath + 'api/services/app/Order/ApplyDiscount',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ DiscountCode: discountCode, TotalAmount: parseFloat($subtotal.data('value') || 0), OrderItems: orderItems }),
            success: res => {
                if (res && res.discountAmount > 0) {
                    $discountRow.show(); $discountAmount.text(formatCurrency(res.discountAmount)).data('value', res.discountAmount);
                    $discountId.val(res.discountId); $discountCodeInput.val(res.discountCode);
                    $discountMessage.text('Áp dụng mã giảm giá thành công!').removeClass('text-danger').addClass('text-success');
                } else resetDiscountInput();
                updateFinalAmount();
            },
            error: resetDiscountInput
        });
    });

    $('#clearDiscountBtn').click(resetDiscountAll);
    $('#btnChooseVoucher').click(() => $('#voucherModal').modal('show'));
    $(document).on('change', '.discount-radio', function () { $discountCodeInput.val($(this).data('discount-code')); $('#voucherModal').modal('hide'); $('#applyDiscountBtn').trigger('click'); });

    updateShippingFee();
});

// Địa chỉ
$(document).ready(() => {
    let locations = [];
    $.getJSON('/data/vietnam_provinces.json', data => { locations = data; locations.forEach(p => $('#Province').append(`<option value="${p.name}">${p.name}</option>`)); });
    $('#Province').change(() => {
        const province = locations.find(p => p.name === $('#Province').val());
        const districtSelect = $('#District').empty().append('<option value="">Chọn Quận/Huyện</option>');
        $('#Ward').empty().append('<option value="">Chọn Xã/Phường</option>');
        if (province) province.districts.forEach(d => districtSelect.append(`<option value="${d.name}">${d.name}</option>`));
    });
    $('#District').change(() => {
        const province = locations.find(p => p.name === $('#Province').val());
        const district = province?.districts.find(d => d.name === $('#District').val());
        const wardSelect = $('#Ward').empty().append('<option value="">Chọn Xã/Phường</option>');
        district?.wards.forEach(w => wardSelect.append(`<option value="${w.name}">${w.name}</option>`));
    });
    $('#btnPlaceOrder').click(() => {
        const detail = $('#DetailAddress').val().trim(), ward = $('#Ward').val(), district = $('#District').val(), province = $('#Province').val();
        $('#DetailAddress').val([detail, ward, district, province].filter(Boolean).join(', '));
    });
});

// OTP
$(document).ready(() => {
    let currentPhone = '', isOtpVerified = false;

    $('#checkoutForm').on('submit', e => { if (!isOtpVerified) { e.preventDefault(); showOtpModal(); } });

    function showOtpModal() {
        const phone = $('#RecipientPhone').val();
        if (!phone) return alert('Vui lòng nhập số điện thoại nhận hàng'), $('#RecipientPhone').focus();
        $('#targetPhoneNumber').text(phone); $('#otpPhoneNumber').val(phone); $('#otpModal').modal('show'); resetOtpForm();
    }

    function resetOtpForm() { $('#otpSendSection').show(); $('#otpVerifySection').hide(); $('#otpMessage').html(''); $('#otpCode').val(''); isOtpVerified = false; }

    $('#btnSendOtp').click(() => {
        const phone = $('#otpPhoneNumber').val(); if (!phone) return showOtpMessage('Vui lòng nhập số điện thoại', 'danger');
        currentPhone = phone; $('#btnSendOtp').html('<i class="fas fa-spinner fa-spin me-2"></i>Đang gửi...').prop('disabled', true);
        $.ajax({
            url: '/Orders/SendOtp', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ phoneNumber: phone }),
            success: res => {
                if (res.success) {
                    $('#otpSendSection').hide(); $('#otpVerifySection').show(); showOtpMessage(res.message, 'success'); $('#otpCode').focus();
                    if (res.otpCode) $('#otpDebugInfo').html(`<strong>DEBUG OTP:</strong> ${res.otpCode}`);
                } else showOtpMessage(res.message, 'danger');
            },
            error: () => showOtpMessage('Lỗi kết nối, vui lòng thử lại', 'danger'),
            complete: () => $('#btnSendOtp').html('<i class="fas fa-paper-plane me-2"></i>Gửi mã OTP').prop('disabled', false)
        });
    });

    $('#btnVerifyOtp').click(() => {
        const code = $('#otpCode').val();

        if (!code || code.length !== 6) {
            return showOtpMessage('Vui lòng nhập mã OTP 6 số', 'danger');
        }

        // Gọi service VerifyOtpAsync trước khi submit form
        abp.services.app.otp.verifyOtp({
            phoneNumber: currentPhone, // số điện thoại hiện tại
            otpCode: code
        })
            .done(res => {
                if (res.success) {
                    // Nếu OTP đúng, submit form
                    isOtpVerified = true;
                    $('#OtpCode').val(code);
                    $('#otpModal').modal('hide');
                    $('#checkoutForm')[0].submit();
                } else {
                    // Nếu OTP sai, hiển thị lỗi ngay trên modal
                    showOtpMessage(res.message, 'danger');
                }
            })
            .fail(() => {
                showOtpMessage('Lỗi kết nối, vui lòng thử lại', 'danger');
            });
    });

    $('#btnResendOtp').click(() => $('#btnSendOtp').click());

    function showOtpMessage(msg, type) {
        if (!msg) {
            // Nếu không có nội dung, xóa vùng thông báo luôn
            $('#otpMessage').html('');
            return;
        }

        $('#otpMessage').html(`
        <div class="alert ${type === 'success' ? 'alert-success' : 'alert-danger'}">
            ${msg}
        </div>
    `);
    }
    $('#otpModal').on('hidden.bs.modal', () => { if (!isOtpVerified) resetOtpForm(); });
});
