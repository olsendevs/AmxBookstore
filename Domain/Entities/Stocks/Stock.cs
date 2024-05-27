using AmxBookstore.Domain.Entities;
using AmxBookstore.Domain.Validation;

namespace Domain.Entities.Stocks
{
    public class Stock : Entity
    {
        public Guid BookId { get; private set; }
        public int Quantity { get; private set; }

        protected Stock() { }
        public Stock(Guid bookId, int quantity, Guid id = default)
        {
            Id = id == default ? Guid.NewGuid() : id;
            ValidateDomain(bookId, quantity);
        }

        private void ValidateDomain(Guid bookId, int quantity)
        {
            DomainExceptionValidation.When(bookId == Guid.Empty, "Invalid book ID. Book ID is required");
            DomainExceptionValidation.When(quantity < 0, "Invalid quantity. Quantity cannot be negative");
            BookId = bookId;
            Quantity = quantity;
        }

        public void UpdateQuantity(int quantity)
        {
            Quantity = quantity;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
