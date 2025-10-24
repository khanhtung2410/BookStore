(function ($) {
    var _bookService = abp.services.app.book,
        l = abp.localization.getSource('Bookstore'),
        _$modal = $('#BookCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#BooksTable');

    var _$booksTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _bookService.getAllBooks,
            inputFilter: function () {
                return $('#BooksSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$booksTable.draw(false)
            }
        ],
        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            {
                targets: 0,
                className: 'control',
                defaultContent: '',
            },
            {
                targets: 1,
                data: 'title',
                sortable: false
            },
            {
                targets: 2,
                data: 'author',
                sortable: false
            },
            {
                targets: 3,
                data: 'genre',
                sortable: false,
            },
            {
                targets: 4,
                data: null,
                sortable: false,
                autoWidth: false,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        `   <button type="button" class="btn btn-sm bg-secondary edit-book" data-book-id="${row.id}" data-toggle="modal" data-target="#BookUpdateModal">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        '   </button>',
                        `   <button type="button" class="btn btn-sm bg-danger delete-book" data-book-id="${row.id}" data-book-title="${row.title}">`,
                        `       <i class="fas fa-trash"></i> ${l('Delete')}`,
                        '   </button>'
                    ].join('');
                }
            }
        ]
    })
    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
        loadGenres('#BookCreateModal');
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    //Delete book
    $(document).on('click', '.delete-book', function () {
        var bookId = $(this).attr('data-book-id');
        var bookTitle = $(this).attr('data-book-title');
        deleteBook(bookId, bookTitle);
    })
    function deleteBook(bookId, bookTitle) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                bookTitle
            ),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _bookService.deleteBook({
                        id: bookId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$booksTable.ajax.reload();
                    });
                }
            }
        );
    }
    //Search
    $('.btn-search').on('click', (e) => {
        _$booksTable.ajax.reload();
    });

    $('.btn-clear').on('click', (e) => {
        $('input[name=Keyword]').val('');
        $('input[name=Genre][value=""]').prop('checked', true);
        _$booksTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$booksTable.ajax.reload();
            return false;
        }
    });
    //Update
    $(document).on('click', '.edit-book', function (e) {
        e.preventDefault();

        var bookId = $(this).data('book-id');
        var $editionSelect = $('#selectEdition');

        _bookService.getBook(bookId).done(function (book) {
            $('#BookUpdateModal input[name="Id"]').val(book.id);
            $('#BookUpdateModal input[name="Title"]').val(book.title);
            $('#BookUpdateModal input[name="Author"]').val(book.author);
            $('#BookUpdateModal textarea[name="Description"]').val(book.description);

            loadGenres('#BookUpdateModal', book.genre);
            $editionSelect.empty().append('<option value="">-- Select an edition --</option>');

            if (book.editions) {
                book.editions.forEach(ed => {
                    const formatName = ed.format === 0 ? 'Hardcover' : 'Paperback';
                    $editionSelect.append(
                        `<option value="${ed.id}"
                        data-format="${formatName}"
                        data-isbn="${ed.isbn}"
                        data-publisher="${ed.publisher}"
                        data-publisheddate="${ed.publishedDate}">
                        ${formatName} - ${ed.isbn}
                    </option>`
                    );
                });

                // Auto-select the first edition and trigger change to fill details
                $editionSelect.val(book.editions[0].id).trigger('change');
            }
        });
    });

    $('#selectEdition').on('change', function () {
        const selectedOption = $(this).find('option:selected');
        if (!selectedOption.val()) {
            // Clear fields if no selection
            $('#editionFields #format').val('');
            $('#editionFields #isbn').val('');
            $('#editionFields #publisher').val('');
            $('#editionFields #publishedDate').val('');
            return;
        }

        $('#editionFields #format').val(selectedOption.data('format'));
        $('#editionFields #isbn').val(selectedOption.data('isbn'));
        $('#editionFields #publisher').val(selectedOption.data('publisher'));

        const rawDate = selectedOption.data('publisheddate'); // "2017-12-11T00:00:00"
        const dateOnly = rawDate ? rawDate.split('T')[0] : '';
        $('#editionFields #publishedDate').val(dateOnly);
    });

    abp.event.on('book.edited', (data) => {
        _$booksTable.ajax.reload();
    })

    //Create
    _$form.find('.save-button').click(function (e) {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var book = _$form.serializeFormToObject();
        book.Genre = parseInt(book.Genre)

        delete book.Format
        delete book.ISBN
        delete book.PublishedDate
        delete book.Publisher

        book.Editions = [];
        $('#editionsContainer .edition-item').each(function () {
            const $ed = $(this);
            book.Editions.push({
                Format: parseInt($ed.find('[name=Format]').val()),
                ISBN: $ed.find('[name=ISBN]').val(),
                Publisher: $ed.find('[name=Publisher]').val(),
                PublishedDate: $ed.find('[name=PublishedDate]').val()
            });
        });

        abp.ui.setBusy(_$modal);

        _bookService
            .createBook(book)
            .done(function () {
                _$modal.modal('hide');
                _$form[0].reset();
                abp.notify.info(l('SavedSuccessfully'));
                _$booksTable.ajax.reload();
            })
            .always(function () {
                abp.ui.clearBusy(_$modal);
            });
    });

    // Add new edition block
    $('#btnAddEdition').on('click', function () {
        const editionHtml = `
        <div class="edition-item border rounded p-3 mb-3">
            <div class="row">
                <div class="col-md-3">
                    <label class="fw-bold">${l('Format')}</label>
                    <select name="Format" class="form-select">
                        <option value="0">Hardcover</option>
                        <option value="1">Paperback</option>
                    </select>
                </div>
                <div class="col-md-3">
                    <label class="fw-bold">${l('ISBN')}</label>
                    <input type="text" name="ISBN" class="form-control" />
                </div>
                <div class="col-md-3">
                    <label class="fw-bold">${l('Publisher')}</label>
                    <input type="text" name="Publisher" class="form-control" />
                </div>
                <div class="col-md-3">
                    <label class="fw-bold">${l('PublishedDate')}</label>
                    <input type="date" name="PublishedDate" class="form-control" />
                </div>
            </div>
            <div class="text-end mt-2">
                <button type="button" class="btn btn-danger btn-sm btn-remove-edition">${l('Remove')}</button>
            </div>
        </div>`;
        $('#editionsContainer').append(editionHtml);
    });

    // Remove edition block
    $(document).on('click', '.btn-remove-edition', function () {
        $(this).closest('.edition-item').remove();
    });


    async function loadGenres(modalSelector, selectedGenre) {
        try {
            const genres = await abp.services.app.book.getBookGenre();

            const $genreSelect = $(`${modalSelector} select[name="Genre"]`);
            $genreSelect.empty(); // clear old options

            // Add placeholder
            $genreSelect.append('<option value="">-- Select Genre --</option>');

            // Populate genre options
            genres.forEach(g => {
                const $option = $('<option></option>')
                    .val(g.value)
                    .text(g.text);
                if (parseInt(g.value) === Number(selectedGenre)) {

                    $option.prop('selected', true);
                }

                $genreSelect.append($option);
            });
        } catch (err) {
            console.error('Failed to load genres', err);
            $(`${modalSelector} select[name="Genre"]`)
                .html('<option value="">Failed to load genres</option>');
        }
    }

    //Import
    //$(document).on('click', '.btn-import-books', function (e) {
    //)
    //}
})(jQuery)