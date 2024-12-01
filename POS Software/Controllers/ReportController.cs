using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace POS_Software.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User is not logged in or invalid.");
            }

            var userRoles = _unitOfWork.ApplicationUser.GetUserRoles(currentUserId) ?? new List<string>();
            List<Order> completeOrders;

            if (userRoles.Contains("Admin"))
            {
                // Admin: Fetch all completed orders
                completeOrders = _unitOfWork.Order
                    .GetAll(o => o.Balance == 0, includeProperties: "OrderItems,Store")
                    .ToList();
            }
            else
            {
                // Manager or Cashier: Filter completed orders by storeId
                var stores = _unitOfWork.Store
                    .GetAll(s => s.CashierId.ToString() == currentUserId || s.ManagerId.ToString() == currentUserId)
                    .ToList();

                if (!stores.Any())
                {
                    return View(new List<Order>()); // Return empty list if no stores are linked
                }

                var storeIds = stores.Select(s => s.Id).ToList();

                completeOrders = _unitOfWork.Order
                    .GetAll(o => o.Balance == 0 && storeIds.Contains(o.StoreId), includeProperties: "OrderItems,Store")
                    .ToList();
            }

            return View(completeOrders ?? new List<Order>());
        }


        [HttpGet]
        public IActionResult ReportDownload(Guid orderId)
        {
            // Retrieve the order using the OrderId
            var order = _unitOfWork.Order.Get(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction("Index");
            }
            
            // Generate the PDF after payment update
            // Retrieve all order items for the given orderId
            var cartData = _unitOfWork.OrderItem.GetList(o => o.OrderId == orderId).ToList();

            // Generate the PDF after payment update
            var pdfData = GenerateOrderPdf(order, cartData);

            // Return the PDF as a file
            return File(pdfData, "application/pdf", $"Invoice_{order.InvoiceNumber}_Updated.pdf");
        }

        private byte[] GenerateOrderPdf(Order order, List<OrderItem> cartData)
        {
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);

                document.Open();

                // Step 1: Add Header with Store Details
                var headerFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD); // Larger font
                var regularFont = FontFactory.GetFont("Arial", 12); // Slightly larger regular font
                var smallFont = FontFactory.GetFont("Arial", 10); // Adjusted smaller font for footer
                var boldFont = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD); // Bold font for emphasis

                document.Add(new iTextSharp.text.Paragraph("POS Software", headerFont) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
                document.Add(new iTextSharp.text.Paragraph($"Sample Address\nTel: {order.Phone}\nEmail: {order.Email}", regularFont) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });

                document.Add(new iTextSharp.text.Paragraph("\n")); // Spacing

                // Step 2: Add Invoice Title
                var titleFont = FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD); // Larger title font
                document.Add(new iTextSharp.text.Paragraph("SALES INVOICE", titleFont) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });

                document.Add(new iTextSharp.text.Paragraph("\n")); // Spacing

                var store = _unitOfWork.Store.Get(s => s.Id.ToString() == order.StoreId.ToString());

                // Fetch Cashier Name
                var cashier = _unitOfWork.ApplicationUser.Get(u => u.Id.ToString() == order.CashierId);

                // Step 3: Add Order and Store Details
                var detailsTable = new iTextSharp.text.pdf.PdfPTable(2) { WidthPercentage = 100 };
                detailsTable.AddCell($"SALES POINT: {store?.Name ?? "New Store"}");
                detailsTable.AddCell($"CASHIER: {cashier?.Name ?? "Unknown"}");
                detailsTable.AddCell($"INVOICE NO: {order.InvoiceNumber}");
                detailsTable.AddCell($"CUSTOMER: {order.CustomerName}");
                detailsTable.AddCell($"DATE: {order.Date:yyyy-MM-dd}");
                detailsTable.AddCell($"TIME: {order.Time:hh:mm:ss tt}");
                document.Add(detailsTable);

                document.Add(new iTextSharp.text.Paragraph("\n")); // Spacing

                // Step 4: Add Items Table
                var itemsTable = new iTextSharp.text.pdf.PdfPTable(3) { WidthPercentage = 100 };
                itemsTable.SetWidths(new float[] { 3, 1, 1 }); // Adjust column widths

                // Add headers with bold font
                var cellStyle = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph("ITEM", boldFont));
                itemsTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("ITEM", boldFont)));
                itemsTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("QUANTITY", boldFont)));
                itemsTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("AMOUNT", boldFont)));

                // Add rows
                foreach (var item in cartData)
                {
                    var product = _unitOfWork.Product.Get(p => p.Id == item.ProductId);

                    itemsTable.AddCell(product?.Name ?? "Unknown Product");
                    itemsTable.AddCell(item.Quantity.ToString());
                    itemsTable.AddCell(item.Amount.ToString("C"));
                }

                document.Add(itemsTable);

                // Step 5: Add Summary Section with larger font
                document.Add(new iTextSharp.text.Paragraph("\n"));
                document.Add(new iTextSharp.text.Paragraph($"Total: {order.TotalAmount:C}", boldFont));
                document.Add(new iTextSharp.text.Paragraph($"Discount: {order.Discount:C}", boldFont));
                document.Add(new iTextSharp.text.Paragraph($"Payable: {order.PayableAmount:C}", boldFont));
                document.Add(new iTextSharp.text.Paragraph($"Paid: {order.PaidAmount:C}", boldFont));
                document.Add(new iTextSharp.text.Paragraph($"Due: {order.Balance:C}", boldFont));
                document.Add(new iTextSharp.text.Paragraph($"Paid Via: {order.PaymentMode}", boldFont));

                // Step 6: Add Footer Section
                document.Add(new iTextSharp.text.Paragraph("\n"));
                document.Add(new iTextSharp.text.Paragraph("Copyright © 2024 POS Software", smallFont) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
                document.Add(new iTextSharp.text.Paragraph("Made by Maruf Hasnat", smallFont) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });

                document.Close();

                return ms.ToArray();
            }
        }

    }
}
