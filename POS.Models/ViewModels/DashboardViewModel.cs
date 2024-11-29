using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Today's metrics
        public decimal TodaySale { get; set; } // Total paid amount received today
        public decimal TodayRevenue { get; set; } // Total payable amount (expected revenue) today

        // Overall metrics
        public decimal TotalSale { get; set; } // Cumulative paid amount received
        public decimal TotalRevenue { get; set; } // Cumulative payable amount (expected revenue)

        // Additional details (optional)
        public int TodayOrdersCount { get; set; } // Number of orders placed today
        public int TotalOrdersCount { get; set; } // Total number of orders placed

        public string? StoreName {  get; set; }
    }
}
