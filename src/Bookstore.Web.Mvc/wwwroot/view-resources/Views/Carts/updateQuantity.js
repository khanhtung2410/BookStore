document.addEventListener("DOMContentLoaded", () => {
    const decreaseBtns = document.getElementsByClassName("btn-minus-quantity");
    const increaseBtns = document.getElementsByClassName("btn-add-quantity");

    // Loop through all rows of quantity controls
    for (let i = 0; i < increaseBtns.length; i++) {
        const decreaseBtn = decreaseBtns[i];
        const increaseBtn = increaseBtns[i];

        // Find the matching input field in the same parent container
        const quantityInput = increaseBtn
            .closest("td")
            .querySelector('input[name="NewQuantity"]');
        const cartItemId = increaseBtn
            .closest("td")
            .querySelector('input[name="CartItemId"]').value;

        const min = parseInt(quantityInput.min) || 1;
        const max = parseInt(quantityInput.max) || 9999;

        // Validation function
        const validateQuantity = () => {
            let value = parseInt(quantityInput.value);
            if (isNaN(value)) {
                abp.notify.error("Please enter a valid number for quantity.");
                quantityInput.value = min;
                return false;
            }
            if (value < min) {
                abp.notify.error(`Quantity cannot be less than ${min}.`);
                quantityInput.value = min;
                return false;
            }
            if (value > max) {
                abp.notify.error(`Quantity cannot be more than ${max}.`);
                quantityInput.value = max;
                return false;
            }
            return true;
        };

        // Decrease button
        decreaseBtn.addEventListener("click", () => {
            let currentQuantity = parseInt(quantityInput.value) || min;
            if (currentQuantity > min) {
                quantityInput.value = currentQuantity - 1;
                if (validateQuantity())
                    quantityInput.dispatchEvent(new Event("change"));
            }
        });

        // Increase button
        increaseBtn.addEventListener("click", () => {
            let currentQuantity = parseInt(quantityInput.value) || min;
            quantityInput.value = currentQuantity + 1;
            if (validateQuantity())
                quantityInput.dispatchEvent(new Event("change"));
        });

        // Input change handler
        quantityInput.addEventListener("change", () => {
            if (validateQuantity()) {
                const newQuantity = parseInt(quantityInput.value);
                abp.services.app.cart.updateCartItemQuantity({
                    cartItemId: cartItemId,
                    newQuantity: newQuantity
                }).done(function (result) {
                    //const row = quantityInput.closest("tr");
                    //const bookPrice = row.querySelector("td:nth-child(5)");
                    //const newSubtotal = newQuantity * parseFloat(bookPrice.dataset.unitPrice);
                    //console.log(newSubtotal);
                    //row.querySelector("td:nth-child(6)").innerText =
                    //    newSubtotal.toLocaleString("vi-VN", { style: "currency", currency: "VND" });
                    //const totalPriceElement = document.querySelector("#total-price");
                    //totalPriceElement.innerText =
                    //    result.totalPrice.toLocaleString("vi-VN", { style: "currency", currency: "VND" });
                });
            }
        });
    }
});
