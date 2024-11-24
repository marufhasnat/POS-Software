$(function () {
    loadCategoryTable();
});

var categoryTable;

function loadCategoryTable() {

    categoryTable = $('#categoryData').DataTable({
        responsive: true,
        "ajax": { url: '/category/getall' },
        "columns": [
            {
                "data": null, // Serial number column
                "render": function (data, type, row, meta) {
                    return meta.row + 1; // Display row index + 1 for serial number
                },
                "width": "10%"
            },
            { data: 'categoryName', "width": "30%" },
            //{ data: 'createdAt', "width": "25%" },
            {
                data: 'createdAt',
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
                        hour: 'numeric', hour12: true, timeZone: 'Asia/Dhaka'
                    };

                    // Format date and time
                    const formattedDate = date.toLocaleDateString('en-US', dateOptions);
                    const formattedTime = date.toLocaleTimeString('en-US', timeOptions);

                    // Return the formatted string
                    return `${formattedDate}, Time: ${formattedTime.toLowerCase()}`;
                },
                "width": "25%",
            },
            {
                data: 'id',
                "render": function (data, type, row) {
                    return `<div class="d-flex justify-content-center">
                                <div class="w-75 btn-group" role="group">
                                    <button onclick="openUpdateModal('${data}', '${row.categoryName}')"
                                            class="btn btn-warning text-white btn-no-shadow me-2" data-bs-toggle="modal" data-bs-target="#categoryUpdateModal">
                                       <i class="fas fa-edit"></i> &nbsp;Edit
                                    </button>
                                    <button onClick="openDeleteModal('${data}', '${row.categoryName}')"
                                       class="btn btn-danger btn-no-shadow ms-2" data-bs-toggle="modal" data-bs-target="#categoryDeleteModal">
                                       <i class="fas fa-trash"></i> &nbsp;Delete
                                    </button>
                                </div>
                            </div>`;
                },
                "width": "35%"
            }
        ],
        "columnDefs": [
            { "className": "dt-left", "targets": 0 } // Apply left alignment to the serial number column (first column)
        ]
    });
}

function openUpdateModal(id, name) {
    // Set the hidden input field and category name input
    $('#updateCategoryId').val(id);
    $('#updateCategoryName').val(name);

    // Optionally set the form action URL, but since the action URL doesn't change here, it's not strictly necessary
    $('#updateCategoryForm').attr('action', `/category/upsert/${id}`);
}

function openDeleteModal(id) {
    // Set the hidden input field and category name input
    $('#deleteCategoryId').val(id);

    // Optionally set the form action URL, but since the action URL doesn't change here, it's not strictly necessary
    $('#deleteCategoryForm').attr('action', `/category/delete/${id}`);
}

