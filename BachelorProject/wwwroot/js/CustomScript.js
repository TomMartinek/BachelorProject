function confirmDelete(id, isDeleteClicked) {
    var deleteSpan = 'deleteSpan_' + id;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + id;

    if (isDeleteClicked) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deleteSpan).show();
        $('#' + confirmDeleteSpan).hide();
    }
}