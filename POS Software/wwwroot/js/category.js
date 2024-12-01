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
                "data": null,
                "render": function (data, type, row, meta) {
                    return meta.row + 1;
                },
                "width": "10%"
            },
            { data: 'categoryName', "width": "30%" },
            {
                data: 'createdAt',
                "render": function (data) {
                    if (!data) return "";

                    const date = new Date(data);
                    const dateOptions = { day: 'numeric', month: 'short', year: 'numeric' };
                    const timeOptions = { hour: 'numeric', hour12: true, timeZone: 'Asia/Dhaka' };
                    const formattedDate = date.toLocaleDateString('en-US', dateOptions);
                    const formattedTime = date.toLocaleTimeString('en-US', timeOptions);

                    return `${formattedDate}, Time: ${formattedTime.toLowerCase()}`;
                },
                "width": "25%"
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
                                    <button onClick="openDeleteModal('${data}')"
                                       class="btn btn-danger btn-no-shadow ms-2" data-bs-toggle="modal" data-bs-target="#categoryDeleteModal">
                                       <i class="fas fa-trash"></i> &nbsp;Delete
                                    </button>
                                </div>
                            </div>`;
                },
                "width": "35%",
                "visible": userRole === "Admin" // Conditionally display the column based on the user's role
            }
        ],
        "columnDefs": [
            { "className": "dt-left", "targets": 0 } // Apply left alignment to the serial number column (first column)
        ]
    });
}

function openUpdateModal(id, name) {
    $('#updateCategoryId').val(id);
    $('#updateCategoryName').val(name);
    $('#updateCategoryForm').attr('action', `/category/upsert/${id}`);
}

function openDeleteModal(id) {
    $('#deleteCategoryId').val(id);
    $('#deleteCategoryForm').attr('action', `/category/delete/${id}`);
}
