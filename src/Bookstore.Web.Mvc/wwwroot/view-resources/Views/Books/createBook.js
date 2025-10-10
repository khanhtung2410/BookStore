$(document).ready(function () {
    const container = $('#edition-container');
    const addButton = $('#addEditionBtn');
    let editionIndex = container.children('.edition-item').length || 0;

    // Add new edition block
    addButton.on('click', function () {
        const newEditionHtml = `
            <div class="edition-item border rounded p-3 mb-3 bg-light">
                <h6 class="fw-semibold">Edition #${editionIndex + 1}</h6>
                <div class="row g-3">
                    <div class="col-md-6">
                        <label class="form-label fw-semibold">Format</label>
                        <select name="Editions[${editionIndex}].Format" class="form-select">
                            <option value="Hardcover">Hardcover</option>
                            <option value="Paperback">Paperback</option>
                        </select>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label fw-semibold">Published Date</label>
                        <input type="date" name="Editions[${editionIndex}].PublishedDate" class="form-control" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label fw-semibold">Publisher</label>
                        <input type="text" name="Editions[${editionIndex}].Publisher" class="form-control" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label fw-semibold">ISBN</label>
                        <input type="text" name="Editions[${editionIndex}].ISBN" class="form-control" />
                    </div>
                </div>

                <h6 class="mt-4 text-muted">Inventory</h6>
                <div class="row g-3">
                    <div class="col-md-4">
                        <label class="form-label fw-semibold">Stock</label>
                        <input type="number" name="Editions[${editionIndex}].Inventory.StockQuantity" class="form-control" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label fw-semibold">Buy Price</label>
                        <input type="number" step="0.01" name="Editions[${editionIndex}].Inventory.BuyPrice" class="form-control" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label fw-semibold">Sell Price</label>
                        <input type="number" step="0.01" name="Editions[${editionIndex}].Inventory.SellPrice" class="form-control" />
                    </div>
                </div>
                <button type="button" class="btn btn-danger btn-sm mt-3 remove-edition-btn">
                    <i class="bi bi-trash"></i> Remove
                </button>
            </div>
        `;
        container.append(newEditionHtml);
        editionIndex++;
    });

    // Remove edition block
    container.on('click', '.remove-edition-btn', function () {
        $(this).closest('.edition-item').remove();
    });

    // Form submission
    $('#createBookForm').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);

        $.ajax({
            url: form.attr('action'),
            method: 'POST',
            data: form.serialize(),
            success: function (response) {
                abp.notify.success("Book created successfully!");
                setTimeout(function () {
                    window.location.href = '/Books';
                }, 500);
            },
            error: function (xhr) {
                abp.notify.error("Error creating book");
                console.log(xhr.responseText);
            }
        });
    });
});
