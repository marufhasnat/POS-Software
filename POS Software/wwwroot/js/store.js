$(function () {
    loadStoreTable();
});

var storeTable;

function loadStoreTable() {
    storeTable = $('#storeData').DataTable({
        responsive: true,
        "ajax": { url: '/store/getall' },
        "columns": [
            {
                "data": null, // Serial number column
                "render": function (data, type, row, meta) {
                    return meta.row + 1; // Display row index + 1 for serial number
                },
                "width": "5%"
            },
            { data: 'name', "width": "15%" },
            { data: 'manager.name', "width": "12%" },
            { data: 'manager.email', "width": "10%" },
            { data: 'cashier.name', "width": "12%" },
            { data: 'cashier.email', "width": "10%" },
            { data: 'location', "width": "15%" },
            { data: 'phone', "width": "6%" },
            {
                data: 'status',
                "render": function (data, type, row) {
                    // Ensure correct base URL (add area if applicable)

                    if (data === true) {
                        return `<span class="badge bg-success no-hover">Open</span>`;
                    } else if (data === false) {
                        return `<span class="badge bg-danger no-hover">Closed</span>`;
                    } else {
                        return `<span class="badge bg-secondary no-hover">Unknown</span>`;
                    }
                },
                "width": "5%"
            },
            {
                data: 'id',
                "render": function (data, type, row) {
                    return `<div class="d-flex">
                                <div class="w-75 btn-group" role="group">
                                    <button onclick="openUpdateModal('${data}', '${row.name}', '${row.manager ? row.manager.id : ''}', '${row.cashier ? row.cashier.id : ''}', '${row.location}', '${row.phone}', '${row.status}')"
                                            class="btn btn-warning text-white btn-no-shadow me-2" data-bs-toggle="modal" data-bs-target="#storeUpdateModal">
                                       <i class="fas fa-edit"></i>
                                    </button>
                                    <button onClick="openDeleteModal('${data}')"
                                       class="btn btn-danger btn-no-shadow ms-2" data-bs-toggle="modal" data-bs-target="#storeDeleteModal">
                                       <i class="fas fa-trash"></i>
                                    </button>
                                </div>
                            </div>`;
                },
                "width": "10%"
            }
        ],
        "columnDefs": [
            { "className": "dt-left", "targets": [0, 7] } // Apply left alignment to the serial number column (first column)
        ]
    });
}

function openUpdateModal(id, name, managerId, cashierId, location, phone, status) {
    // Reset previous selections
    $('#updateManagerId').val(null);
    $('#updateCashierId').val(null);

    // Populate update modal fields
    $('#updateStoreId').val(id);
    $('#updateStoreName').val(name);
    $('#updateStoreLocation').val(location);
    $('#updateStorePhone').val(phone);
    $('#updateStoreStatus').val(status);

    // Set the selected manager
    if (managerId) {
        $('#updateManagerId').val(managerId);
    }

    // Set the selected cashier
    if (cashierId) {
        $('#updateCashierId').val(cashierId);
    }

    // Optionally set the form action URL
    $('#updateStoreForm').attr('action', `/store/upsert/${id}`);
}

function openDeleteModal(id) {
    // Populate delete modal fields with store data
    $('#deleteStoreId').val(id);

    // Optionally set the form action URL
    $('#deleteStoreForm').attr('action', `/store/delete/${id}`);
}


