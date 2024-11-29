namespace POS_Software.DTOs
{
    public class OrderListDTO
    {
        public class OrderDto
        {
            public Guid OrderId { get; set; }
            public string CustomerName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal Discount {  get; set; }
            public decimal PayableAmount { get; set; }
            public decimal PaidAmount { get; set; }
            public decimal Balance { get; set; }
            public string PaymentMode { get; set; }
            public string InvoiceNumber { get; set; }
            public string Cashier { get; set; }
            public ICollection<OrderItemDto> OrderItems { get; set; }
        }

        public class OrderItemDto
        {
            public Guid OrderItemId { get; set; }
            public string ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }

    }
}
