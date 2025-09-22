$(function () {

    function getTotalQuantity() {
        var totalQty = 0;
        $("input[name='quantity']").each(function () {
            var qty = parseInt($(this).val()) || 1;
            var maxStock = parseInt($(this).attr('max')) || qty;

            if (qty < 1) qty = 1;
            if (qty > maxStock) {
                alert("Số lượng vượt quá tồn kho! Tối đa: " + maxStock);
                $(this).val(maxStock);
                qty = maxStock;
            }

            // Chỉ cộng nếu sản phẩm được check (nếu có checkbox)
            var checkbox = $(this).closest("tr").find("input[name='selectProduct']");
            if (checkbox.length === 0 || checkbox.is(":checked")) {
                totalQty += qty;
            }
        });
        return totalQty;
    }

    // Khi submit form
    $('#buyNowForm').submit(function (e) {
        var totalQty = getTotalQuantity();
        $("#buyNowQuantity").val(totalQty);

        if (totalQty <= 0) {
            alert("Bạn chưa chọn sản phẩm nào!");
            e.preventDefault();
            return false;
        }
    });

    // Nếu có checkbox selectProduct, cập nhật tổng số lượng khi tick/uncheck
    $("input[name='selectProduct']").change(function () {
        $("#buyNowQuantity").val(getTotalQuantity());
    });

    // Nếu có checkbox Select All
    $("#selectAll").change(function () {
        var checked = $(this).is(":checked");
        $("input[name='selectProduct']").prop("checked", checked);
        $("#buyNowQuantity").val(getTotalQuantity());
    });
});
