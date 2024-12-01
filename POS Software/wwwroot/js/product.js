$(function () {
    loadProductTable();
});

var storeTable;

function loadProductTable() {
    storeTable = $('#productData').DataTable({
        responsive: true,
        "ajax": { url: '/product/getall' },
        "columns": [
            {
                "data": null, // Serial number column
                "render": function (data, type, row, meta) {
                    return meta.row + 1; // Display row index + 1 for serial number
                },
                "width": "5%"
            },
            { data: 'name', "width": "15%" },
            { data: 'category.categoryName', "width": "12%" },
            {
                data: 'expiryDate',
                "render": function (data) {
                    if (!data) return ""; // If no date, return an empty string

                    // Convert the date string to a Date object
                    const date = new Date(data);

                    // Define options for the date portion
                    const dateOptions = {
                        day: 'numeric', month: 'short', year: 'numeric'
                    };

                    // Define options for the time portion
                    const timeOptions = {
                        hour: 'numeric', minute: 'numeric', hour12: true, timeZone: 'Asia/Dhaka'
                    };

                    // Format date and time
                    const formattedDate = date.toLocaleDateString('en-US', dateOptions);

                    // Return the formatted string
                    return `${formattedDate}`;
                },
                "width": "10%"
            },
            { data: 'sellPrice', "width": "12%" },
            { data: 'quantity', "width": "10%" },
            { data: 'store.name', "width": "15%" },
            {
                data: 'status',
                "render": function (data, type, row) {
                    // Ensure row.quantity exists
                    if (row.quantity === undefined) {
                        return `<span class="badge bg-secondary no-hover">Unknown</span>`;
                    }

                    // Determine stock status
                    if (row.quantity === 0) {
                        return `<span class="badge bg-danger no-hover">Out of Stock</span>`;
                    } else if (row.quantity < 20) {
                        return `<span class="badge bg-warning no-hover">Low Stock</span>`;
                    } else {
                        return `<span class="badge bg-success no-hover">In Stock</span>`;
                    }
                },
                "width": "5%"
            },
            {
                data: 'id',
                "render": function (data, type, row) {
                    // Show the action buttons only if the user is Admin or Manager
                    if (userRole === "Admin" || userRole === "Manager") {
                        return `<div class="d-flex">
                                    <div class="w-75 btn-group" role="group">
                                        <button onclick="openUpdateModal('${data}', '${row.name}', '${row.description}', '${row.batch}', '${row.costPrice}', '${row.sellPrice}', '${row.quantity}', '${row.supplier ? row.supplier.id : ''}', '${row.category ? row.category.id : ''}', '${row.store ? row.store.id : ''}', '${row.manufactureDate}', '${row.expiryDate}')"
                                                class="btn btn-warning text-white btn-no-shadow me-2" data-bs-toggle="modal" data-bs-target="#productUpdateModal">
                                           <i class="fas fa-edit"></i>
                                        </button>
                                        ${userRole === "Admin" ?
                                `<button onClick="openDeleteModal('${data}')"
                                                class="btn btn-danger btn-no-shadow ms-2" data-bs-toggle="modal" data-bs-target="#productDeleteModal">
                                                <i class="fas fa-trash"></i>
                                            </button>`
                                : ''}
                                    </div>
                                </div>`;
                    }
                    return ''; // Return empty string if the user is not Admin or Manager
                },
                "width": "10%",
                "visible": userRole === "Admin" || userRole === "Manager"
            }
        ],
        "columnDefs": [
            { "className": "dt-left", "targets": [0, 4, 5] } // Apply left alignment to the serial number column (first column)
        ]
    });
}

function openUpdateModal(id, name, description, batch, costPrice, sellPrice, quantity, supplierId, categoryId, storeId, manufactureDate, expiryDate) {
    // Reset previous selections
    $('#updateSupplierId').val(null);
    $('#updateCategoryId').val(null);
    $('#updateStoreId').val(null);

    // Convert date strings to the required format (YYYY-MM-DD)
    const formatDate = (dateString) => {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toISOString().split('T')[0]; // Extract the date portion
    };

    // Populate update modal fields
    $('#updateProductId').val(id);
    $('#updateProductName').val(name);
    $('#updateProductDescription').val(description);
    $('#updateProductBatch').val(batch);
    $('#updateCostPrice').val(costPrice);
    $('#updateSellPrice').val(sellPrice);
    $('#updateQuantity').val(quantity);
    $('#updateMFTDate').val(formatDate(manufactureDate));
    $('#updateExpiryDate').val(formatDate(expiryDate));

    // Set the selected supplier
    if (supplierId) {
        $('#updateSupplierId').val(supplierId);
    }

    // Set the selected category
    if (categoryId) {
        $('#updateCategoryId').val(categoryId);
    }

    // Set the selected store
    if (storeId) {
        $('#updateStoreId').val(storeId);
    }

    // Optionally set the form action URL
    $('#updateProductForm').attr('action', `/product/upsert/${id}`);
}

function openDeleteModal(id) {
    // Populate delete modal fields with store data
    $('#deleteProductId').val(id);

    // Optionally set the form action URL
    $('#deleteProductForm').attr('action', `/product/delete/${id}`);
}


