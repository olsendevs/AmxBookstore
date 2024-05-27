namespace AmxBookstore.Application.DTOs
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public List<OrderItemDTO>? Products { get; set; }
        public Guid SellerId { get; set; }
        public Guid ClientId { get; set; }
        public string Status { get; set; }

        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Deleted { get; set; }
    }

    public class OrderItemDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
