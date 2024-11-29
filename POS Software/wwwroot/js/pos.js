$(document).ready(function () {
    let cart = [];
    let grandTotal = 0;
    let discountPercentage = 0;
    let finalTotal = 0;

    // Prevent "Enter" from submitting the form in the discount field
    $("#discount").keydown(function (event) {
        if (event.key === "Enter") {
            event.preventDefault(); // Prevent form submission
            $("#applyDiscount").click(); // Trigger the Apply Discount button click
        }
    });

    // Prevent "Enter" from submitting the form in the discount field
    $("#quntity").keydown(function (event) {
        if (event.key === "Enter") {
            event.preventDefault(); // Prevent form submission
        }
    });

    // Fetch Products
    $.ajax({
        url: '/pos/getall',
        method: 'GET',
        success: function (response) {
            console.log("Fetched Products:", response.data);
            response.data.forEach(product => {
                const productName = product.name;
                const category = product.category?.categoryName || "Unknown";
                const sellPrice = product.sellPrice;
                const quantity = product.quantity;
                const productId = product.id;

                $("#product").append(
                    $('<option>')
                        .val(productId)
                        .text(`${productName} >> ${category}`)
                        .data('sellPrice', sellPrice)
                        .data('quantity', quantity)
                        .data('category', category)
                );
            });
        },
        error: function (xhr, status, error) {
            console.error("Error fetching products:", error);
        }
    });

    $("#addToCart").click(function () {
        const selectedOption = $("#product").find(":selected");
        const productName = selectedOption.text().split(">>")[0].trim();
        const category = selectedOption.data("category") || "Unknown";
        const unitPrice = parseFloat(selectedOption.data("sellPrice"));
        const stockQty = parseInt(selectedOption.data("quantity"));
        const productId = selectedOption.val(); // Explicitly get ProductId
        const quantity = 1;
        const subtotal = unitPrice * quantity; // Calculate Amount

        if (!productId || isNaN(unitPrice) || isNaN(stockQty)) {
            alert("Invalid product selection.");
            return;
        }

        const existingItemIndex = cart.findIndex(item => item.productId === productId);
        if (existingItemIndex > -1) {
            alert("Product is already in the cart.");
            return;
        }

        cart.push({
            productId,  // Include ProductId
            productName,
            category,
            stockQty,
            unitPrice,
            quantity,
            amount: subtotal
        });

        console.log("Updated Cart with ProductId and Amount:", cart);
        updateCartTable();
    });


    // Update Cart Table
    function updateCartTable() {
        grandTotal = 0;
        let cartRows = "";

        cart.forEach((item, index) => {
            grandTotal += item.amount;
            cartRows += `
                <tr>
                    <td>
                        ${item.productName}
                    </td>
                    <td>${item.category}</td>
                    <td>${item.stockQty}</td>
                    <td>${item.unitPrice.toFixed(2)}</td>
                    <td>
                        <input type="number" value="${item.quantity}" min="1" id="quntity" class="form-control cart-qty" data-index="${index}">
                    </td>
                    <td>${item.amount.toFixed(2)}</td>
                    <td>
                        <button class="btn btn-danger btn-sm remove-item" data-index="${index}">Remove</button>
                    </td>
                </tr>
            `;
        });

        $("#cartTable").html(cartRows);
        $("#grandTotal").text(grandTotal.toFixed(2));
        $("#hiddenGrandTotal").val(grandTotal.toFixed(2));
        attachEvents();
        console.log("Grand Total Updated:", grandTotal);
    }

    // Apply Discount
    $("#applyDiscount").click(function () {
        const discountInput = $("#discount").val();
        discountPercentage = parseFloat(discountInput);

        if (isNaN(discountPercentage) || discountPercentage < 0 || discountPercentage > 100) {
            alert("Please enter a valid discount percentage.");
            return;
        }

        finalTotal = grandTotal - (grandTotal * discountPercentage / 100);
        updateFinalTotal();
        console.log("Discount Applied:", discountPercentage, "Final Total:", finalTotal);
    });

    // Update Final Total
    function updateFinalTotal() {
        $("#finalTotal").text(finalTotal.toFixed(2));
        $("#hiddenFinalTotal").val(finalTotal.toFixed(2));
    }

    // Attach Events
    function attachEvents() {
        $(".remove-item").click(function () {
            const index = $(this).data("index");
            cart.splice(index, 1);
            updateCartTable();
        });

        $(".cart-qty").keydown(function (event) {
            // Prevent form submission or page reload on Enter key
            if (event.key === "Enter") {
                event.preventDefault();
            }
        }).change(function () {
            const index = $(this).data("index");

            const newQty = parseInt($(this).val());
            const stockQty = cart[index].stockQty; // Get the stock quantity from the cart

            if (newQty < 1) {
                alert("Quantity must be at least 1.");
                return;
            }

            if (newQty > stockQty) {
                alert("Quantity cannot exceed the stock quantity.");
                $(this).val(stockQty); // Reset the input value to stock quantity
                cart[index].quantity = stockQty; // Update the cart quantity to stock quantity
                cart[index].amount = stockQty * cart[index].unitPrice; // Recalculate the amount
            } else {
                // Update quantity and recalculate amount (subtotal)
                cart[index].quantity = newQty;
                cart[index].amount = newQty * cart[index].unitPrice; // Recalculate the amount
            }

            updateCartTable(); // Refresh the cart table
        });

    }


    $("#posForm").submit(function (event) {
        if (cart.length === 0) {
            alert("Cart is empty. Please add products.");
            event.preventDefault();
            return;
        }

        // Update stock quantity in the cart before submitting
        cart = cart.map(item => {
            item.stockQty -= item.quantity; // Reduce the stock quantity by the quantity purchased
            return item;
        });

        // Pass updated cart data to the hidden field
        $("#cartData").val(JSON.stringify(cart));
    });

    // Clear Button Functionality: Reload the page
    $("#clearForm").click(function () {
        location.reload(); // Reload the page to reset everything
    });

});
