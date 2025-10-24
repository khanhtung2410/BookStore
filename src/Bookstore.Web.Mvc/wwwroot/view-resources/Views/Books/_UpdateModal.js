(function ($) {
    var _bookService = abp.services.app.book,
        l = abp.localization.getSource('Bookstore'),
        _$modal = $('#BookUpdateModal'),
        _$form = _$modal.find('form');
    function save() {
        if (!_$form.valid()) {
            return;
        }
        var book = _$form.serializeFormToObject();

        book.Genre = parseInt(book.Genre);
        book.Id = parseInt(book.Id, 10);

        book.Editions = [
            {
                Id: parseInt(book.SelectedEditionId, 10),
                BookId: parseInt(book.Id, 10),
                Format: book.Format === "Hardcover" ? 0 : 1,
                ISBN: book.ISBN,
                Publisher: book.Publisher,
                PublishedDate: book.PublishedDate
            }
        ];

        delete book.Format;
        delete book.ISBN;
        delete book.Publisher;
        delete book.SelectedEditionId;
        delete book.PublishedDate;

        abp.ui.setBusy(_$form);
        _bookService.updateBook(book).done(function () {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('book.edited', book);
        })
            .fail(function (error) {
                abp.notify.error(error.message);
            })
            .always(function () {
                abp.ui.clearBusy(_$form);
            });
    }

    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        save();
    });

    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            save();
        }
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=text]:first').focus();
    });

})(jQuery);
