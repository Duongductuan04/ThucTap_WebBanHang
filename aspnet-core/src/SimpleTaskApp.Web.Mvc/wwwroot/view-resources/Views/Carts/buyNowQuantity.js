﻿$(function () {
  // ======== MODAL (chỉ chạy nếu tồn tại) ========
  const modalEl = document.getElementById('quantityModal');
  const modalQuantity = $('#modalQuantity');
  let currentAction = '';
  const stockQuantity = modalQuantity.length ? parseInt(modalQuantity.attr('max')) || 100 : 100;
  let modal;
  if (modalEl) {
    modal = new bootstrap.Modal(modalEl);
  }

  // ======== HÀM TÍNH TỔNG GIỎ HÀNG ========
  function updateCartTotals() {
    let total = 0;
    $('.selectItem:checked').each(function () {
      const $row = $(this).closest('tr');
      const price = parseFloat($(this).data('price')) || 0;
      const quantity = parseInt($row.find('.cartQuantity').val()) || 1;
      total += price * quantity;
    });
    $('#selectedTotal').text(
      new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(total)
    );
  }

  // ======== XỬ LÝ SUBMIT FORM GIỎ HÀNG ========
  $('#addToCartForm').submit(function (e) {
    e.preventDefault();
    const $form = $(this);
    const token = $form.find('input[name="__RequestVerificationToken"]').val();
    const mobilePhoneId = parseInt($form.find('input[name="mobilePhoneId"]').val());
    const quantity = parseInt($form.find('input[name="quantity"]').val()) || 1;
    let colorVal = $form.find('input[name="MobilePhoneColorId"]').val();
    const colorId = colorVal && colorVal !== "null" && colorVal !== "" ? parseInt(colorVal) : null;
    abp.ui.setBusy($form);

    $.ajax({
      url: $form.attr('action'),
      type: 'POST',
      data: {
        __RequestVerificationToken: token,
        mobilePhoneId: mobilePhoneId,
        MobilePhoneColorId: colorId, // có thể null
        quantity: quantity
      },
      success: function (response) {
        if (response.success) {
          abp.notify.success(response.message || 'Thêm vào giỏ hàng thành công.');
          setTimeout(() => location.reload(), 500);
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

  // ======== Tăng/Giảm số lượng ========
  $(document).on('click', '.btn-qty', function () {
    const $btn = $(this);
    const $input = $btn.siblings('.cartQuantity, #buyNowQuantity, #modalQuantity');
    let val = parseInt($input.val()) || 1;
    const max = parseInt($input.attr('max')) || 100;
    const min = parseInt($input.attr('min')) || 1;

    if ($btn.hasClass('increase')) val = Math.min(val + 1, max);
    else if ($btn.hasClass('decrease')) val = Math.max(val - 1, min);

    $input.val(val);

    const cartId = $input.data('cart-id');
    let price = $input.data('price');
    if (typeof price === 'string') price = parseFloat(price.replace(/\./g, '')) || 0;
    else price = parseFloat(price) || 0;

    if (cartId && typeof updateQuantityUrl !== 'undefined') {
      $.post(updateQuantityUrl, { cartId, quantity: val }, function () {
        $input.closest('tr').find('.item-total').text((val * price).toLocaleString('vi-VN') + ' VNĐ');
        updateCartTotals();
      });
    } else {
      updateCartTotals();
    }
  });

  // ======== Chọn tất cả / chọn từng sản phẩm ========
  const $selectAll = $('#selectAll');
  const $items = $('.selectItem');
  $selectAll.on('change', function () {
    $items.prop('checked', this.checked);
    updateCartTotals();
  });
  $items.on('change', updateCartTotals);

  // ======== Chọn màu (nếu có) ========
  let selectedColorId = null;
  $(document).on('change', 'input[name="selectedColor"]', function () {
    selectedColorId = $(this).data('color-id') || null;
    const imageUrl = $(this).data('image-url');
    if (imageUrl) $('#mainProductImage').attr('src', imageUrl);
    $('.color-radio-box').removeClass('selected');
    $(this).closest('.color-radio-box').addClass('selected');
  });

  // ======== Mua ngay / Thêm vào giỏ ========
  $('.add-to-cart-btn, .buy-now-btn').click(function () {
    if (!modal) return;
    currentAction = $(this).data("action");
    modalQuantity.val(1);
    modal.show();
  });

  $('#confirmAction').click(function () {
    if (!modal) return;

    const quantity = parseInt(modalQuantity.val()) || 1;
    if (quantity < 1 || quantity > stockQuantity) {
      alert("Số lượng không hợp lệ!");
      return;
    }

    const selected = $('input[name="selectedColor"]:checked');
    const colorId = selected.length ? selected.data('color-id') : null;

    const $form = currentAction === "addToCart" ? $('#addToCartForm') : $('#buyNowForm');
    $form.find('input[name="MobilePhoneColorId"]').val(colorId); // ✅ đúng tên trùng controller

    if (currentAction === "addToCart") {
      $form.find('#cartQuantity').val(quantity);
    } else if (currentAction === "buyNow") {
      $form.find('#buyNowQuantity').val(quantity);
    }

    $form.submit(); // ✅ submit form đúng cách
    modal.hide();
  });


  // ======== Đóng modal ========
  $('.btn-secondary').click(function () {
    if (modal) modal.hide();
  });

  // ======== Khởi tạo tổng tiền ========
  updateCartTotals();
});
