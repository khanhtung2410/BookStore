$(function () {
    const container = $('#edition-container');
    const addButton = $('#addEditionBtn');
    let editionIndex = container.children('.edition-item').length; // continue after loaded editions
    // Function to create a new edition block
    function addEdition(index) {
        const newEditionHtml = `
        <div class="edition-item border rounded p-3 mb-3 bg-light">
            <h6 class="fw-semibold">Edition #${index + 1}</h6>
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label fw-semibold">Format</label>
                    <select name="Editions[${index}].Format" class="form-select" required>
                        <option value="Hardcover">Hardcover</option>
                        <option value="Paperback">Paperback</option>
                    </select>
                </div>
                <div class="col-md-4">
                    <label class="form-label fw-semibold">Published Date</label>
                    <input type="date" name="Editions[${index}].PublishedDate" class="form-control" required />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-semibold">Publisher</label>
                    <input type="text" name="Editions[${index}].Publisher" class="form-control" required />
                </div>
                <div class="col-md-4">
                    <label class="form-label fw-semibold">ISBN</label>
                    <input type="text" name="Editions[${index}].ISBN" class="form-control" required />
                </div>
            </div>

            <h6 class="mt-4 text-muted">Inventory</h6>
            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label fw-semibold">Stock</label>
                    <input type="number" name="Editions[${index}].Inventory.StockQuantity" class="form-control" min="0" required />
                </div>
                <div class="col-md-4">
                    <label class="form-label fw-semibold">Buy Price</label>
                    <input type="number" step="0.01" name="Editions[${index}].Inventory.BuyPrice" class="form-control" min="0" required />
                </div>
                <div class="col-md-4">
                    <label class="form-label fw-semibold">Sell Price</label>
                    <input type="number" step="0.01" name="Editions[${index}].Inventory.SellPrice" class="form-control" min="0" required />
                </div>
            </div>

            <button type="button" class="btn btn-danger btn-sm mt-3 remove-edition-btn">
                <i class="bi bi-trash"></i> Remove
            </button>
        </div>`;
        container.append(newEditionHtml);
        $.validator.unobtrusive.parse(container);
    }

    // Add edition dynamically
    addButton.on('click', function () {
        addEdition(editionIndex);
        editionIndex++;
    });

    // Remove edition
    container.on('click', '.remove-edition-btn', function () {
        $(this).closest('.edition-item').remove();
        editionIndex--;
    });

    // Form validation & AJAX submission
    $('#updateBookForm').validate({
        ignore: [],
        rules: {
            Title: "required",
            Author: "required",
            "Genre": "required"
        },
        messages: {
            Title: "Please enter the book title",
            Author: "Please enter the author name",
            Genre: "Please select a genre"
        },
        errorClass: 'text-danger small',
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        },
        submitHandler: function (form) {
            $.ajax({
                url: $(form).attr('action'),
                method: 'POST',
                data: $(form).serialize(),
                success: function () {
                    abp.notify.success("Book updated successfully!");
                    setTimeout(function () {
                        window.location.href = '/Books';
                    }, 500);
                },
                error: function (xhr) {
                    abp.notify.error("Error updating book");
                    console.log(xhr.responseText);
                }
            });
        }
    });
});
