function confirmApplyVoucher(id, isApplyVoucherClicked) {
    var applyVoucherSpan = 'applyVoucherSpan_' + id;
    var confirmApplyVoucherSpan = 'confirmApplyVoucherSpan_' + id;

    if (isApplyVoucherClicked) {
        $('#' + applyVoucherSpan).hide();
        $('#' + confirmApplyVoucherSpan).show();
    } else {
        $('#' + applyVoucherSpan).show();
        $('#' + confirmApplyVoucherSpan).hide();
    }
}