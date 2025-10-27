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
        var bookId = $(this).data('book-id');
        var bookTitle = $(this).data('book-title');
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
    window.allEditions = [];
    $(document).on('click', '.edit-book', function (e) {
        e.preventDefault();
        var bookId = $(this).data('book-id');

        var $editionSelect = $('#selectEdition');

        _bookService.getBook(bookId).done(function (book) {
            $('#BookUpdateModal input[name="Id"]').val(book.id);
            $('#BookUpdateModal input[name="Title"]').val(book.title);
            $('#BookUpdateModal input[name="Author"]').val(book.author);
            $('#BookUpdateModal textarea[name="Description"]').val(book.description);

            var $modal = $('#BookUpdateModal');
            loadBookImages(bookId, $modal);
            loadGenres('#BookUpdateModal', book.genre);

            window.allEditions = book.editions || [];

            $editionSelect.empty().append(`<option value="">-- ${l("SelectAnEdition")} --</option>`);

            window.allEditions.forEach(ed => {
                const formatValue = ed.format;
                const formatName = ed.format === 0 ? l('Hardcover') : l('Paperback');
                $editionSelect.append(
                    `<option value="${ed.id}" 
                        data-format="${formatValue}" 
                        data-isbn="${ed.isbn}" 
                        data-publisher="${ed.publisher}" 
                        data-publisheddate="${ed.publishedDate}">
                ${formatName} - ${ed.isbn}</option>`);
            });

            // Auto-select the first edition and trigger change to fill details
            if (window.allEditions.length > 0) {
                $editionSelect.val(allEditions[0].id).trigger('change');
            }
        });
    });

    $('#selectEdition').on('change', function () {
        const selectedId = parseInt($(this).val(), 10);
        const selectedEdition = window.allEditions.find(ed => ed.id === selectedId);

        if (!selectedEdition) {
            $('#format,#isbn,#publisher,#publishedDate').val('');
            return;
        }

        $('#format').val(selectedEdition.format);
        $('#isbn').val(selectedEdition.isbn);
        $('#publisher').val(selectedEdition.publisher);

        const dateOnly = selectedEdition.publishedDate ? selectedEdition.publishedDate.split('T')[0] : '';
        $('#publishedDate').val(dateOnly);

        // optional: update hidden input for save
        $('#SelectedEditionId').val(selectedId);
    });


    abp.event.on('book.edited', (data) => {
        _$booksTable.ajax.reload();
    })

    // Create

    // Custom rule for ISBN length
    $.validator.addMethod("isbnLength", function (value, element) {
        return this.optional(element) || value.length === 10 || value.length === 13;
    }, l('InvalidISBNFormat', ''));

    _$form.validate({
        ignore: [], // include hidden fields for edition validation
        rules: {
            Title: {
                required: true,
                maxlength: 250
            },
            Author: {
                required: true,
                maxlength: 200
            },
            Description: {
                required: true,
                maxlength: 1000
            },
            Genre: {
                required: true
            },
            'Editions[0].Format': {
                required: true
            },
            'Editions[0].Publisher': {
                required: true,
                maxlength: 200
            },
            'Editions[0].ISBN': {
                required: true,
                digits: true,
                isbnLength: true
            },
            'Editions[0].PublishedDate': {
                required: true,
                dateISO: true,
                max: function () {
                    var today = new Date();
                    today.setFullYear(today.getFullYear() + 1);
                    return today.toISOString().split('T')[0];
                }
            },
            'BuyPrice': {
                required: false,
                number: true,
                min: 0
            },
            'SellPrice': {
                required: false,
                number: true,
                min: 0
            },
            'StockQuantity': {
                required: false,
                number: true,
                min: 0
            }
        },
        messages: {
            Title: {
                required: l('TitleIsRequired'),
                maxlength: l('TitleMaxLengthExceeded', l('Title'), 250)
            },
            Author: {
                required: l('AuthorIsRequired'),
                maxlength: l('AuthorMaxLengthExceeded', l('Author'), 200)
            },
            Description: {
                required: l('DescriptionIsRequired'),
                maxlength: l('DescriptionMaxLengthExceeded', l('Description'), 1000)
            },
            Genre: {
                required: l('GenreIsRequired')
            },
            'Editions[0].Format': {
                required: l('FormatIsRequired')
            },
            'Editions[0].Publisher': {
                required: l('PublisherIsRequired'),
                maxlength: l('PublisherMaxLengthExceeded', l('Publisher'), 200)
            },
            'Editions[0].ISBN': {
                required: l('ISBNIsRequired'),
                digits: l('InvalidISBNFormat', ''),
            },
            'Editions[0].PublishedDate': {
                required: l('PublishedDateIsRequired'),
                dateISO: l('InvalidPublishedDateFuture')
            },
            'BuyPrice': {
                min: l('BuyPriceNonNegative')
            },
            'SellPrice': {
                min: l('SellPriceNonNegative')
            },
            'StockQuantity': {
                min: l('StockQuantityNonNegative')
            }
        },
        errorPlacement: function (error, element) {
            if (element.closest('.edition-item').length) {
                error.appendTo(element.closest('.edition-item'));
            } else {
                error.insertAfter(element);
            }
        },
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        }
    })

    _$form.find('.save-button').click(async function (e) {
        e.preventDefault();

        if ($('#editionsContainer .edition-item').length === 0) {
            abp.notify.error(l('BookMustHaveAtLeastOneEdition'));
            return;
        }

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
                        <option value="0">${l('Hardcover')}</option>
                        <option value="1">${l('Paperback')}</option>
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

        _$form.validate().settings.rules[`Editions[${editionIndex}].Format`] = { required: true };
        _$form.validate().settings.rules[`Editions[${editionIndex}].ISBN`] = {
            required: true,
            digits: true,
            isbnLength: true
        };
        _$form.validate().settings.rules[`Editions[${editionIndex}].Publisher`] = {
            required: true,
            maxlength: 200
        };
        _$form.validate().settings.rules[`Editions[${editionIndex}].PublishedDate`] = {
            required: true,
            dateISO: true,
            max: function () {
                var today = new Date();
                today.setFullYear(today.getFullYear() + 1);
                return today.toISOString().split('T')[0];
            }
        };
    });

    // Remove edition block
    $(document).on('click', '.btn-remove-edition', function () {
        $(this).closest('.edition-item').remove();
        $('#editionsContainer .edition-item').each(function (i, el) {
            $(el).find('[name]').each(function () {
                const nameParts = $(this).attr('name').split('.');
                const field = nameParts[1];
                $(this).attr('name', `Editions[${i}].${field}`);
            });
        });
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
            abp.notify.error(l('MaxTenImagesAllowed'));
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


    function loadBookImages(bookId, $modal) {
        _bookService.getBookImages(bookId).done(function (images) {
            const $preview = $modal.find('#imagePreview');
            $preview.empty();
            images.forEach(img => {
                const $cardWrapper = $(`
                <div class="col-6 mb-3 card-wrapper">
                    <div class="card" data-image-id="${img.id}">
                        <img src="${img.imagePath}" class="card-img-top" style="height:150px; object-fit:cover;" />
                        <div class="card-body p-2 d-flex justify-content-between align-items-center">
                            <span class="small text-truncate" style="max-width:120px;">${img.caption || ''}</span>
                            <button type="button" class="btn btn-sm btn-danger ml-auto">×</button>
                        </div>
                    </div>
                </div>
            `);

                // Mark for deletion, do NOT remove yet
                $cardWrapper.find('button').on('click', function () {
                    $cardWrapper.find('.card').addClass('deleted'); // mark
                    $cardWrapper.hide(); // hide from view
                });

                $preview.append($cardWrapper);
            });
        });
    }

    // View book
    $(document).on('click', '.view-book', function (e) {
        e.preventDefault();
        var bookId = $(this).data('book-id');
        var $editionSelect = $('#selectEditionView');

        _bookService.getBookImages(bookId).done(function (images) {
            const $preview = $("#BookViewModal").find('#imagePreview');
            $preview.empty();
            if (images.length > 0) {
                images.forEach(img => {
                    const $cardWrapper = $(`
                <div class="col-6 mb-3 card-wrapper">
                    <div class="card" data-image-id="${img.id}">
                        <img src="${img.imagePath}" class="card-img-top" style="height:150px; object-fit:cover;" />
                        <div class="card-body p-2 d-flex justify-content-between align-items-center">
                            <span class="small text-truncate" style="max-width:120px;">${img.caption || ''}</span>
                        </div>
                    </div>
                </div>
            `);
                    $preview.append($cardWrapper);
                });
            } else {
                $preview.append(`<p>${l('ThisBookDoesNotHaveImageYet')}</p>`);
            }
        });

        _bookService.getBook(bookId).done(async function (book) {
            // Populate main book details inside the modal
            const genres = await _bookService.getBookGenre();
            let genreText = '-';
            const genreItem = genres.find(g => Number(g.value) === Number(book.genre));
            if (genreItem) genreText = genreItem.text;

            $('#BookViewModal #titleView').text(book.title);
            $('#BookViewModal #authorView').text(book.author);
            $('#BookViewModal #genreView').text(genreText);
            $('#BookViewModal #descriptionView').text(book.description || '-');

            window.allEditions = book.editions || [];

            // Populate editions dropdown
            $editionSelect.empty();
            window.allEditions.forEach(ed => {
                const formatName = ed.format === 0 ? l('Hardcover') : l('Paperback');
                $editionSelect.append(
                    `<option value="${ed.id}" 
                    data-format="${formatName}" 
                    data-isbn="${ed.isbn || 'N/A'}" 
                    data-publisher="${ed.publisher || 'N/A'}" 
                    data-publisheddate="${ed.publishedDate ? ed.publishedDate.split('T')[0] : '-'}">
                    ${formatName} - ${ed.isbn || 'N/A'}
                </option>`
                );
            });

            // Auto-select first edition if available and populate details
            if (window.allEditions.length > 0) {
                const firstEdition = window.allEditions[0];
                $editionSelect.val(firstEdition.id).trigger('change');
            }


            $('#BookViewModal').modal('show');
        });
    });

    // When edition selection changes, populate read-only fields
    $('#selectEditionView').on('change', function () {
        var selected = $(this).find('option:selected');
        $('#formatView').text(selected.data('format') || '-');
        $('#isbnView').text(selected.data('isbn') || '-');
        $('#publisherView').text(selected.data('publisher') || '-');
        $('#publishedDateView').text(selected.data('publisheddate') || '-');
    });


    //Import
    //$(document).on('click', '.btn-import-books', function (e) {
    //)
    //}
})(jQuery)