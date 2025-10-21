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
                        `   <button type="button" class="btn btn-sm bg-secondary edit-book" data-book-id="${row.id}" data-toggle="modal" data-target="#BookEditModal">`,
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
    //Import
    //$(document).on('click', '.btn-import-books', function (e) {
    //)
    //}
})(jQuery)