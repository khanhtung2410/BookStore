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
                render: function (data, type, row, meta) {
                    if (!data) return '';
                    const localized = l('Genre_' + data); // e.g., l('Genre_Mystery')
                    return localized || data; // fallback to raw string if missing
                }
            },
            {
                targets: 4,
                data: null,
                sortable: false,
                autoWidth: false,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        `   <button type="button" class="btn btn-sm bg-info view-book" data-book-id="${row.id}" data-toggle="modal" data-target="#BookViewModal">`,
                        `       <i class="fas fa-eye"></i> ${l('View')}`,
                        '   </button>',
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

        $('#BookUpdateModal').one('shown.bs.modal', function () {
            loadBookImages(bookId);
            loadBookImagesAsLinks(bookId);

        });

        var $editionSelect = $('#selectEdition');

        _bookService.getBook(bookId).done(function (book) {
            $('#BookUpdateModal input[name="Id"]').val(book.id);
            $('#BookUpdateModal input[name="Title"]').val(book.title);
            $('#BookUpdateModal input[name="Author"]').val(book.author);
            $('#BookUpdateModal textarea[name="Description"]').val(book.description);

            loadGenres('#BookUpdateModal', book.genre);
            $editionSelect.empty().append(`<option value="">-- ${l("SelectAnEdition")} --</option>`);

            if (book.editions) {
                book.editions.forEach(ed => {
                    const formatValue = ed.format;
                    const formatName = ed.format === 0 ? l('Hardcover') : l('Paperback');
                    $editionSelect.append(
                        `<option value="${ed.id}"
                        data-format="${formatValue}"
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

    // Create
    _$form.find('.save-button').click(async function (e) {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var book = _$form.serializeFormToObject();
        book.Genre = parseInt(book.Genre);

        delete book.Format;
        delete book.ISBN;
        delete book.PublishedDate;
        delete book.Publisher;

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

        try {
            const bookId = await _bookService.createBook(book);

            if (selectedFiles.length > 0) {
                try {
                    const formData = new FormData();
                    selectedFiles.forEach(f => formData.append('files', f));
                    formData.append('bookId', bookId);

                    await $.ajax({
                        url: '/api/services/app/Book/UploadBookImages',
                        type: 'POST',
                        data: formData,
                        contentType: false,
                        processData: false
                    });
                } catch (imgError) {
                    console.error(imgError);
                    abp.notify.warn(
                        "Book saved, but image upload failed. You can try uploading images again."
                    );
                }
            }

            abp.notify.info(l('SavedSuccessfully'));
            _$modal.modal('hide');
            _$form[0].reset();
            selectedFiles = [];
            $('#imagePreview').empty();
            _$booksTable.ajax.reload();

        } catch (error) {
            console.error(error);
            abp.notify.error(error.message || 'Error saving book');
        } finally {
            abp.ui.clearBusy(_$modal);
        }
    });


    // Add new edition block
    $('#btnAddEdition').on('click', function () {
        const editionHtml = `
        <div class="edition-item border rounded p-3 mb-3">
            <div class="row">
                <div class="col-md-3">
                    <label class="fw-bold">${l('Format')}</label>
                    <select name="Format" class="form-select form-control">
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

    // Image preview
    const MAX_FILES = 10;
    let selectedFiles = [];
    function updateInputFiles() {
        const dataTransfer = new DataTransfer();
        selectedFiles.forEach(f => dataTransfer.items.add(f));
        $('#bookImages')[0].files = dataTransfer.files;
    }

    $('#bookImages').on('change', function () {
        const files = Array.from(this.files);

        if (files.length > MAX_FILES) {
            abp.notify.error(`You can upload a maximum of ${MAX_FILES} images.`);
            $(this).val('');
            selectedFiles = [];
            $('#imagePreview').empty();
            return;
        }

        selectedFiles = files;
        $('#imagePreview').empty();

        $.each(files, function (i, file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                // Bootstrap 4 col-6 (2 per row)
                const $col = $(`
                    <div class="col-6 mb-3">
                        <div class="card">
                            <img src="${e.target.result}" class="card-img-top" style="height:150px; object-fit:cover;" />
                            <div class="card-body p-2 d-flex justify-content-between align-items-center">
                                <span class="small text-truncate" style="max-width: 120px;">${file.name}</span>
                                <button type="button" class="btn btn-sm btn-danger ml-auto">×</button>
                            </div>
                        </div>
                    </div>
                `);

                $col.find('button').on('click', function () {
                    const index = selectedFiles.indexOf(file);
                    if (index > -1) selectedFiles.splice(index, 1);
                    $col.remove();
                    updateInputFiles();
                });

                $('#imagePreview').append($col);
            };
            reader.readAsDataURL(file);
        });
    });

    async function loadGenres(modalSelector, selectedGenre) {
        try {
            const genres = await _bookService.getBookGenre();

            const $genreSelect = $(`${modalSelector} select[name="Genre"]`);
            $genreSelect.empty();

            // Add placeholder
            $genreSelect.append(`<option value="">-- ${l('SelectGenre')} --</option>`);

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
                .html(`<option value="">${l('FailedToLoadGenres')}</option>`);
        }
    }

    function loadBookImages(bookId) {
        _bookService.getBookImages(bookId).done(function (images) {
            console.log(images); // should log array

            if (!images || !images.length) return;

            const $preview = $('#imagePreview');
            $preview.empty();

            images.forEach(img => {
                const $col = $(`
                <div class="col-6 mb-3">
                    <div class="card">
                        <img src="${img.imagePath}" class="card-img-top" style="height:150px; object-fit:cover;" />
                        <div class="card-body p-2 d-flex justify-content-between align-items-center">
                            <span class="small text-truncate" style="max-width: 120px;">${img.caption || ''}</span>
                            <button type="button" class="btn btn-sm btn-danger ml-auto">×</button>
                        </div>
                    </div>
                </div>
            `);

                $col.find('button').on('click', function () {
                    $col.remove();
                });

                $preview.append($col);
            });
        });
    }

    function loadBookImagesAsLinks(bookId) {
        _bookService.getBookImages(bookId).done(function (images) {
            console.log(images); // confirm array

            if (!images || !images.length) {
                $('#imageLinks').html('<p>No images found for this book.</p>');
                return;
            }

            const $links = $('#imageLinks');
            $links.empty();

            images.forEach(img => {
                const $a = $(`
                <div class="mb-2">
                    <a href="${img.imagePath}" target="_blank">${img.caption || img.imagePath}</a>
                </div>
            `);
                $links.append($a);
            });
        });
    }


    //Import
    //$(document).on('click', '.btn-import-books', function (e) {
    //)
    //}
})(jQuery)