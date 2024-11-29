using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using POS.Models.ViewModels;


namespace POS_Software.Controllers
{
    public class POSController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public POSController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Get all products
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            OrderViewModel orderViewModel = new OrderViewModel()
            {
                Products = products.Select(product => new SelectListItem
                {
                    Value = product.Id.ToString(),
                    Text = $"{product.Name} >> {product.Category.CategoryName}",
                }).ToList()
            };

            return View(orderViewModel);
        }

        [HttpPost]
        public IActionResult Create(OrderViewModel viewModel)
        {
            // Validate PaidAmount against PayableAmount
            if (viewModel.Order.PaidAmount > viewModel.Order.PayableAmount)
            {
                TempData["error"] = "Paid amount cannot exceed the final amount.";
                return RedirectToAction("Index");
            }

            var cartData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItem>>(viewModel.CartData);

            if (cartData == null || !cartData.Any())
            {
                TempData["error"] = "Cart is empty.";
                return RedirectToAction("Index");
            }

            foreach (var item in cartData)
            {
                var product = _unitOfWork.Product.Get(p => p.Id == item.ProductId);

                if (product == null)
                {
                    TempData["error"] = $"Product with ID {item.ProductId} not found.";
                    return RedirectToAction("Index");
                }

                if (product.Quantity < item.Quantity)
                {
                    TempData["error"] = $"Insufficient stock for product '{product.Name}'.";
                    return RedirectToAction("Index");
                }

                product.Quantity -= item.Quantity;
                _unitOfWork.Product.Update(product);
            }

            var currentUserId = _userManager.GetUserId(User);
            var cashierId = currentUserId;

            var store = _unitOfWork.Store.Get(s => s.CashierId.ToString() == currentUserId);

            if (store == null)
            {
                TempData["error"] = "Store associated with the current user not found.";
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                StoreId = store.Id,
                CashierId = cashierId,
                CustomerName = viewModel.Order.CustomerName,
                Email = viewModel.Order.Email,
                Phone = viewModel.Order.Phone,
                Date = DateTime.Now.Date,
                Time = DateTime.Now,
                TotalAmount = cartData.Sum(item => item.Amount),
                Discount = viewModel.Order.Discount,
                PayableAmount = viewModel.Order.PayableAmount,
                PaidAmount = viewModel.Order.PaidAmount,
                Balance = viewModel.Order.PayableAmount - viewModel.Order.PaidAmount,
                PaymentMode = viewModel.Order.PaymentMode,
                InvoiceNumber = GenerateInvoiceNumber(),
                OrderItems = new List<OrderItem>()
            };

            _unitOfWork.Order.Add(order);
            _unitOfWork.Save();

            foreach (var item in cartData)
            {
                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Amount = item.Amount
                };
                _unitOfWork.OrderItem.Add(orderItem);
            }

            _unitOfWork.Save();

            // Generate the PDF
            var pdfData = GenerateOrderPdf(order, cartData);

            // Return the PDF as a file
            return File(pdfData, "application/pdf", $"Invoice_{order.InvoiceNumber}.pdf");
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


        // Generate Invoice Number
        private string GenerateInvoiceNumber()
        {
            var random = new Random();
            return $"INV-{random.Next(100000, 999999).ToString()}";
        }

        #region API CALLS

        // https://localhost:7230/pos/getall => to get all the propoerties of Product in json for using in datatable
        [HttpGet]
        public IActionResult GetAll()
        {
            var currentUserId = _userManager.GetUserId(User);

            // Fetch the store associated with the current user (CashierId matches currentUserId)
            var store = _unitOfWork.Store.Get(s => s.CashierId.ToString() == currentUserId);

            // Ensure the store exists for this user
            if (store == null)
            {
                return Json(new { data = new List<object>(), message = "No store found for the current user." });
            }

            // Fetch products associated with the user's store and include related entities
            var objStoreProductList = _unitOfWork.Product.GetAll(filter: p => p.StoreId == store.Id, includeProperties: "Supplier,Category,Store").ToList();

            // Return the filtered data as JSON
            return Json(new { data = objStoreProductList });
        }

        #endregion
    }
}
