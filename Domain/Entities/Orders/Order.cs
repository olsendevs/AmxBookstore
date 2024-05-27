using AmxBookstore.Domain.Entities;
using AmxBookstore.Domain.Entities.Orders;
using AmxBookstore.Domain.Validation;


namespace Domain.Entities.Orders
{
    public class Order : Entity
    {
        public List<OrderItem> Products { get; private set; }
        public decimal TotalAmount { get; private set; }
        public Guid SellerId { get; private set; }
        public Guid ClientId { get; private set; }
        public OrderStatus Status { get; private set; }

        protected Order() { }

        public Order(List<OrderItem> products, decimal totalAmount, Guid sellerId, Guid clientId, OrderStatus status,  Guid id = default)
        {
            Id = id == default ? Guid.NewGuid() : id;
            ValidateDomain(products, totalAmount, sellerId, clientId, status);
        }

        private void ValidateDomain(List<OrderItem> products, decimal totalAmount, Guid sellerId, Guid clientId, OrderStatus status)
        {
            DomainExceptionValidation.When(products == null || products.Count == 0, "Order must have at least one product");
            DomainExceptionValidation.When(sellerId == Guid.Empty, "Invalid seller ID. Seller ID is required");
            DomainExceptionValidation.When(clientId == Guid.Empty, "Invalid client ID. Client ID is required");

            Products = products;
            TotalAmount = totalAmount;
            SellerId = sellerId;
            ClientId = clientId;
            Status = status;
        }

        public void Update(decimal totalAmount, Guid sellerId, Guid clientId, OrderStatus status)
        {
            TotalAmount = totalAmount;
            SellerId = sellerId;
            ClientId = clientId;
            Status = status;
        }

        public void InsertTotalAmount(decimal totalAmount)
        {
            TotalAmount = totalAmount;
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
