(function ($) {
    var _bookService = abp.services.app.book,
        l = abp.localization.getSource('Bookstore'),
        _$modal = $('#BookUpdateModal'),
        _$form = _$modal.find('form');
        selectedFiles = [];
    const $fileInput = _$modal.find('#bookImages'),
          $preview = _$modal.find('#imagePreview');

    // Image preview
    const MAX_FILES = 10;
    function updateInputFiles() {
        const dataTransfer = new DataTransfer();
        selectedFiles.forEach(f => dataTransfer.items.add(f));
        $fileInput[0].files = dataTransfer.files;
    }

    $fileInput.on('change', function () {
        const files = Array.from(this.files);

        if (files.length > MAX_FILES) {
            abp.notify.error(`You can upload a maximum of ${MAX_FILES} images.`);
            $(this).val('');
            selectedFiles = [];
            $preview.empty();
            return;
        }

        selectedFiles = files;
        $preview.empty();

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

                $preview.append($col);
            };
            reader.readAsDataURL(file);
        });
    });

    $.validator.addMethod("isbnLength", function (value, element) {
        return this.optional(element) || value.length === 10 || value.length === 13;
    }, l('InvalidISBNFormat'));

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
                digits: l('InvalidISBNFormat')
            },
            'Editions[0].PublishedDate': {
                required: l('PublishedDateIsRequired'),
                dateISO: l('InvalidPublishedDateFuture')
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
    });

    async function save() {
        if (!_$form.valid()) return;

        var book = _$form.serializeFormToObject();
        book.Genre = parseInt(book.Genre);
        book.Id = parseInt(book.Id, 10);

        const selectedId = parseInt($('#selectEdition').val(), 10);

        const updatedEdition = {
            Id: selectedId,
            BookId: book.Id,
            Format: parseInt(book.Format, 10),
            ISBN: book.ISBN,
            Publisher: book.Publisher,
            PublishedDate: book.PublishedDate
        };

        const otherEditions = window.allEditions.filter(ed => ed.id !== selectedId);
        book.Editions = [updatedEdition, ...otherEditions];

        delete book.Format;
        delete book.ISBN;
        delete book.Publisher;
        delete book.PublishedDate;

        abp.ui.setBusy(_$form);
        try {
            await _bookService.updateBook(book);

            // handle images here
            let deletedImageIds = [];
            $('#imagePreview .card.deleted').each(function () {
                const id = $(this).data('image-id');
                if (id) deletedImageIds.push(id);
            });
            if (deletedImageIds.length > 0) await _bookService.deleteBookImages({ ids: deletedImageIds });

            if (selectedFiles.length > 0) {
                const formData = new FormData();
                formData.append('bookId', book.Id);
                selectedFiles.forEach(f => formData.append('files', f));
                await $.ajax({
                    url: '/api/services/app/Book/UploadBookImages',
                    type: 'POST',
                    data: formData,
                    contentType: false,
                    processData: false
                });
                selectedFiles = [];
            }

            abp.notify.info(l('SavedSuccessfully'));
            $('#BookUpdateModal').modal('hide');
            abp.event.trigger('book.edited', book);

        } catch (error) {
            console.error(error);
            abp.notify.error(error.message || 'Error updating book');
        } finally {
            abp.ui.clearBusy(_$form);
        }
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
