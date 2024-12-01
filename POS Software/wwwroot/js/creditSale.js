document.addEventListener("DOMContentLoaded", function () {
    // Handle Pay Now button click
    const payNowButtons = document.querySelectorAll(".pay-now-btn");
    payNowButtons.forEach(button => {
        button.addEventListener("click", function () {
            const balance = this.getAttribute("data-balance");
            const orderId = this.getAttribute("data-order-id");

            // Populate modal fields
            document.getElementById("orderBalance").value = balance;
            document.getElementById("orderId").value = orderId;
        });
    });

    // Search and Clear functionality
    //const searchInput = document.getElementById("tableSearch");
    //const table = document.getElementById("creditSaleData").getElementsByTagName("tbody")[0];
    //const clearButton = document.getElementById("clearSearch");

    //searchInput.addEventListener("input", function () {
    //    const filter = searchInput.value.toLowerCase();
    //    const rows = table.getElementsByTagName("tr");

    //    // Toggle the visibility of the clear button
    //    clearButton.style.display = filter ? "block" : "none";

    //    // Filter table rows based on the search query
    //    for (let i = 0; i < rows.length; i++) {
    //        const cells = rows[i].getElementsByTagName("td");
    //        let found = false;
    //        for (let j = 1; j < cells.length; j++) {
    //            if (cells[j] && cells[j].innerText.toLowerCase().includes(filter)) {
    //                found = true;
    //                break;
    //            }
    //        }
    //        rows[i].style.display = found ? "" : "none";
    //    }
    //});

    //// Clear the search input and reset the table
    //clearButton.addEventListener("click", function () {
    //    searchInput.value = "";
    //    clearButton.style.display = "none";

    //    // Reset all table rows to be visible
    //    const rows = table.getElementsByTagName("tr");
    //    for (let i = 0; i < rows.length; i++) {
    //        rows[i].style.display = "";
    //    }
    //});
});