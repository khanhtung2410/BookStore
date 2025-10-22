(function ($) {
    var l = abp.localization.getSource('Bookstore'),
        _$importModal = $('#BooksImportModal'),
        _$importForm = $('#importForm');

    // Handle Import form submit
    _$importForm.submit(function (e) {
        e.preventDefault();

        if (!_$importForm.valid()) {
            return;
        }

        var formData = new FormData(this);

        abp.ui.setBusy(_$importModal);

        $.ajax({
            url: _$importForm.attr('action'),
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (result) {
                _$importModal.modal('hide');
                abp.notify.info(l('ImportedSuccessfully'));
                // Reload your DataTable if needed
                if (typeof _$booksTable !== 'undefined') {
                    _$booksTable.ajax.reload();
                }
            },
            error: function (err) {
                abp.notify.error(l('ImportFailed'));
            },
            complete: function () {
                abp.ui.clearBusy(_$importModal);
            }
        });
    });

    //Get Excel Template button
    $(document).on('click', '#getFormatExcel', function () {
        window.location.href = '/Import/DownloadTemplate';
    });

    // Focus on first input when modal shown
    _$importModal.on('shown.bs.modal', function () {
        _$importModal.find('input:first').focus();
    });

    // Clear form when modal hidden
    _$importModal.on('hidden.bs.modal', function () {
        _$importForm[0].reset();
    });
})(jQuery);
