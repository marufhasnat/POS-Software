// Ensure the model values are passed into a script tag dynamically
window.onload = function () {
    // Fetch data from Razor model
    const todaySale = parseFloat(document.getElementById('todaySaleValue').value) || 0;
    const totalSale = parseFloat(document.getElementById('totalSaleValue').value) || 0;
    const todayRevenue = parseFloat(document.getElementById('todayRevenueValue').value) || 0;
    const totalRevenue = parseFloat(document.getElementById('totalRevenueValue').value) || 0;

    // Worldwide Sales Chart
    var ctx1 = document.getElementById("worldwide-sales").getContext("2d");
    new Chart(ctx1, {
        type: "bar",
        data: {
            labels: ["Today Sale", "Total Sale", "Today Revenue", "Total Revenue"],
            datasets: [{
                label: "Sales & Revenue Data",
                data: [todaySale, totalSale, todayRevenue, totalRevenue],
                backgroundColor: [
                    "rgba(0, 156, 255, 0.7)",
                    "rgba(0, 156, 255, 0.5)",
                    "rgba(0, 156, 255, 0.3)",
                    "rgba(0, 156, 255, 0.2)"
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });

    // Sales & Revenue Chart
    var ctx2 = document.getElementById("salse-revenue").getContext("2d");
    new Chart(ctx2, {
        type: "line",
        data: {
            labels: ["Today Sale", "Total Sale", "Today Revenue", "Total Revenue"],
            datasets: [{
                label: "Sales",
                data: [todaySale, totalSale],
                backgroundColor: "rgba(0, 156, 255, 0.5)",
                borderColor: "rgba(0, 156, 255, 0.9)",
                fill: true
            }, {
                label: "Revenue",
                data: [todayRevenue, totalRevenue],
                backgroundColor: "rgba(0, 156, 255, 0.3)",
                borderColor: "rgba(0, 156, 255, 0.7)",
                fill: true
            }]
        },
        options: {
            responsive: true,
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (tooltipItem) {
                            return tooltipItem.dataset.label + ": $" + tooltipItem.raw.toLocaleString();
                        }
                    }
                }
            }
        }
    });
};
