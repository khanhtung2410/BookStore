document.addEventListener("DOMContentLoaded", () => {
    const editions = window.bookEditions || [];
    const editionSelect = document.getElementById("editionSelect");
    const publisherText = document.getElementById("publisherText");
    const publishedText = document.getElementById("publishedText");
    const isbnText = document.getElementById("isbnText");
    const stockText = document.getElementById("stockText");
    const buyPriceText = document.getElementById("buyPriceText");
    const sellPriceText = document.getElementById("sellPriceText");
 
    function showEdition(id) {
        const edition = editions.find(e => e.Id === parseInt(id));
        if (!edition) return;


        publisherText.textContent = edition.Publisher || "N/A";
        publishedText.textContent = edition.PublishedDate
            ? new Date(edition.PublishedDate).toLocaleDateString('en-GB')
            : "N/A";
        isbnText.textContent = edition.ISBN || "N/A";

        const inv = edition.Inventory;
        stockText.textContent = inv ? inv.StockQuantity : "N/A";
        buyPriceText.textContent = inv ? inv.BuyPrice.toFixed(2) : "N/A";
        sellPriceText.textContent = inv ? inv.SellPrice.toFixed(2) : "N/A";
    }

    if (editionSelect) {
        editionSelect.addEventListener("change", e => {
            showEdition(e.target.value);
        });
        // Initialize with first edition
        if (editions.length > 0) {
            editionSelect.value = editions[0].Id.toString();
            showEdition(editions[0].Id);
        }
    }
});
