using AmxBookstore.Domain.Validation;

namespace AmxBookstore.Domain.Entities.Orders
{
    public class OrderItem 
    {
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }

        public OrderItem() { }
        public OrderItem(Guid productId, int quantity)
        {
            ValidateDomain(productId, quantity);
        }

        private void ValidateDomain(Guid productId, int quantity)
        {
            DomainExceptionValidation.When(productId == Guid.Empty, "Invalid product ID. Product ID is required");
            DomainExceptionValidation.When(quantity <= 0, "Invalid quantity. Quantity must be greater than zero");

            ProductId = productId;
            Quantity = quantity;
        }
    }
}
