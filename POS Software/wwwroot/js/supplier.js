$(function () {
    loadSupplierTable();
});

var supplierTable;

function loadSupplierTable() {
    supplierTable = $('#supplierData').DataTable({
        responsive: true,
        "ajax": { url: '/supplier/getall' },
        "columns": [
            {
                "data": null, // Serial number column
                "render": function (data, type, row, meta) {
                    return meta.row + 1; // Display row index + 1 for serial number
                },
                "width": "5%"
            },
            { data: 'supplierName', "width": "10%" },
            { data: 'company', "width": "10%" },
            { data: 'email', "width": "10%" },
            { data: 'phone', "width": "15%" },
            { data: 'address', "width": "20%" },
            {
                data: 'regDate',
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
                "width": "15%",
            },
            {
                data: 'id',
                "render": function (data, type, row) {
                    return `<div class="d-flex justify-content-center">
                                <div class="w-75 btn-group" role="group">
                                    <button onclick="openUpdateModal('${data}', '${row.supplierName}', '${row.company}', '${row.email}', '${row.phone}', '${row.address}')"
                                            class="btn btn-warning text-white btn-no-shadow me-2" data-bs-toggle="modal" data-bs-target="#supplierUpdateModal">
                                       <i class="fas fa-edit"></i>
                                    </button>
                                    <button onClick="openDeleteModal('${data}')"
                                       class="btn btn-danger btn-no-shadow ms-2" data-bs-toggle="modal" data-bs-target="#supplierDeleteModal">
                                       <i class="fas fa-trash"></i>
                                    </button>
                                </div>
                            </div>`;
                },
                "width": "15%"
            }
        ],
        "columnDefs": [
            { "className": "dt-left", "targets": [0, 4] } // Apply left alignment to the serial number column (first column)
        ]
    });
}

function openUpdateModal(id, name, company, email, phone, address) {
    // Populate update modal fields with supplier data
    $('#updateSupplierId').val(id);
    $('#updateSupplierName').val(name);
    $('#updateCompany').val(company);
    $('#updateEmail').val(email);
    $('#updatePhone').val(phone);
    $('#updateAddress').val(address);

    // Optionally set the form action URL
    $('#updateSupplierForm').attr('action', `/supplier/upsert/${id}`);
}

function openDeleteModal(id) {
    // Populate delete modal fields with supplier data
    $('#deleteSupplierId').val(id);

    // Optionally set the form action URL
    $('#deleteSupplierForm').attr('action', `/supplier/delete/${id}`);
}


