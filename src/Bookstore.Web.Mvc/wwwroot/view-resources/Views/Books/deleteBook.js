document.addEventListener("DOMContentLoaded", function () {
    var deleteModal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
    function getCurrentPage() {
        const params = new URLSearchParams(window.location.search);
        return parseInt(params.get('page')) || 1;
    }

    document.querySelectorAll('.btn-delete-book').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var bookId = this.dataset.id;
            var bookTitle = this.dataset.title;

            document.getElementById('deleteBookMessage').textContent =
                'Are you sure you want to delete "' + bookTitle + '"?';
            document.getElementById('deleteBookId').value = bookId;

            deleteModal.show();
        });
    });

    document.getElementById('confirmDeleteBtn').addEventListener('click', function () {
        var id = document.getElementById('deleteBookId').value;

        $.ajax({
            url: '/Books/Delete/' + id,
            method: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function () {
                deleteModal.hide();
                abp.notify.success("Book deleted successfully!");
                var book = document.querySelector('[data-id="' + id + '"]').closest('.list-group-item');
                if (book) book.remove();
                var remainingBooks = document.querySelectorAll('.list-group-item').length;

                if (remainingBooks === 0) {
                    const currentPage = getCurrentPage();
                    if (currentPage > 1) {
                        // Redirect to previous page
                        window.location.href = `/Books?page=${currentPage - 1}`;
                    } else {
                        // Page 1, no books left → show "No books available"
                        const container = document.querySelector('.list-group');
                        if (container) {
                            container.innerHTML = `
                                <div class="alert alert-info mt-4">
                                    No books available.
                                </div>
                            `;
                        }
                    }
                }
            },
            error: function (xhr) {
                deleteModal.hide();
                abp.notify.error("Error deleting book");
                console.log(xhr.responseText);
            }
        });
    });
});
