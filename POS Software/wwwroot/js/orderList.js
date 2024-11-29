$(function () {
    loadOrderTable();
});

var orderTable;

function loadOrderTable() {

    orderTable = $('#orderData').DataTable({
        responsive: true,
        "ajax": { url: '/orderList/getall' },
        "columns": [
            {
                "data": null, // Serial number column
                "render": function (data, type, row, meta) {
                    return meta.row + 1; // Display row index + 1 for serial number
                },
                "width": "3%"
            },
            { data: 'invoiceNumber', "width": "10%" },
            { data: 'customerName', "width": "10%" },
            { data: 'email', "width": "10%" },
            { data: 'phone', "width": "5%" },
            { data: 'totalAmount', "width": "5%" },
            {
                data: 'discount', "render": function (data, type, row) {
                    return `${data}%`;
                }, "width": "5%" },
            { data: 'payableAmount', "width": "5%" },
            { data: 'paidAmount', "width": "5%" },
            {
                data: 'paymentMode', "render": function (data, type, row) {            
                    return `<span class="badge bg-primary no-hover">${data}</span>`;                    
                }, "width": "5%" },
            {
                data: 'balance', "render": function (data, type, row) {
                    // Determine stock status
                    if (data === 0) {
                        return `<span class="badge bg-success no-hover">Paid</span>`;
                    } else {
                        return `<span class="badge bg-danger no-hover">Due</span>`;
                    }
                }, "width": "5%"
            },
            { data: 'cashier', "width": "10%" }
        ],
        "columnDefs": [
            { "className": "dt-left", "targets": [0, 4, 5, 6, 7, 8] } // Apply left alignment to the serial number column (first column)
        ]
    });
}
