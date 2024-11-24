$(function () {
    loadUserTable();
});

var userTable;

function loadUserTable() {

    userTable = $('#userData').DataTable({
        responsive: true,
        "ajax": { url: '/user/getall' },
        "columns": [
            { data: 'id', visible: false },
            {
                "data": null, // Serial number column
                "render": function (data, type, row, meta) {
                    return meta.row + 1; // Display row index + 1 for serial number
                },
                "width": "3%" 
            },
            { data: 'name', "width": "5%" },
            { data: 'email', "width": "10%" },
            {
                data: 'role',
                "render": function (data, type, row) {
                    // Extract the role if it's an array, otherwise default to an empty string
                    const role = Array.isArray(data) && data.length > 0 ? data[0] : '';

                    // Normalize the role (trim spaces and make lowercase)
                    const normalizedRole = role.toString().trim().toLowerCase();

                    // Default button class
                    let bgClass = 'bg-primary'; // Fallback for unexpected roles

                    // Assign class based on normalized role
                    switch (normalizedRole) {
                        case 'admin':
                            bgClass = 'bg-success'; // Green for Admin
                            break;
                        case 'manager':
                            bgClass = 'bg-warning text-white'; // Yellow with white text for Manager
                            break;
                        case 'cashier':
                            bgClass = 'bg-dark'; // Dark for Cashier
                            break;
                    }

                    // Return the button HTML
                    return `<span class="badge ${bgClass}">${role || 'Unknown Role'}</span>`;
                },
                "width": "10%"
            },
            { data: 'division', "width": "5%" },
            { data: 'city', "width": "5%" },
            { data: 'streetAddress', "width": "17%" },
            { data: 'postalCode', "width": "10%" },
            { data: 'phoneNumber', "width": "15%" },
            {
                data: 'status',
                "render": function (data, type, row) {
                    // Ensure correct base URL (add area if applicable)
                    const baseUrl = '/User/ReverseStatus';

                    if (data === true) {
                        return `<a href="${baseUrl}?id=${row.id}" class="badge bg-success no-hover" style="cursor: pointer">Active</a>`;
                    } else if (data === false) {
                        return `<a href="${baseUrl}?id=${row.id}" class="badge bg-danger no-hover" style="cursor: pointer">Inactive</a>`;
                    } else {
                        return `<a href="${baseUrl}?id=${row.id}" class="badge bg-secondary no-hover" style="cursor: pointer">Unknown</a>`;
                    }
                },
                "width": "5%"
            },
            {
                data: 'email',
                "render": function (data, type, row) {
                    return `<div class="d-flex">
                                <div class="w-75 btn-group" role="group">
                                    <button onclick="openShowModal('${row.id}', '${row.name}', '${data}', '${row.role}', '${row.division}', '${row.city}', '${row.streetAddress}', '${row.postalCode}', '${row.phoneNumber}')"
                                            class="btn btn-primary text-white btn-no-shadow me-2" data-bs-toggle="modal" data-bs-target="#userShowModal">
                                       <i class="fas fa-eye"></i>
                                    </button>
                                    <button onClick="openDeleteModal('${data}', '${row.userName}')"
                                       class="btn btn-danger btn-no-shadow ms-2" data-bs-toggle="modal" data-bs-target="#userDeleteModal">
                                       <i class="fas fa-trash"></i>
                                    </button>
                                </div>
                            </div>`;
                },
                "width": "15%"
            }
        ],
        "columnDefs": [
            { "className": "dt-left", "targets": [1, 8, 9] } // Apply left alignment to the serial number column (first column)
        ]
    });
}

function openShowModal(id, name, email, role, division, city, streetAddress, postalCode, phoneNumber) {
    // Set the hidden input field and user name input
    $('#showUserId').val(id);
    $('#showName').val(name);
    $('#showEmail').val(email);
    $('#showRole').val(role);
    $('#showDivision').val(division);
    $('#showCity').val(city);
    $('#showStreetAddress').val(streetAddress);
    $('#showPostalCode').val(postalCode);
    $('#showPhoneNumber').val(phoneNumber);

}

function openDeleteModal(email) {
    // Set the hidden input field and user name input
    $('#deleteUserEmail').val(email);

    // Optionally set the form action URL, but since the action URL doesn't change here, it's not strictly necessary
    $('#deleteUserForm').attr('action', `/user/delete/${email}`);
}

