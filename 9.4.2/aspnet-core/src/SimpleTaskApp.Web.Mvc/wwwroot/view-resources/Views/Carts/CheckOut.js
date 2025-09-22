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
            const quantity = parseInt(row.querySelector('input[name="quantity"]').value) || 1;
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
}

// Cập nhật tổng tiền khi tải trang
document.addEventListener('DOMContentLoaded', updateTotalAmount);
