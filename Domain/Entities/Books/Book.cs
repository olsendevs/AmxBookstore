using AmxBookstore.Domain.Entities;
using AmxBookstore.Domain.Validation;

namespace Domain.Entities.Books
{
    public class Book : Entity
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public int Pages { get; private set; }
        public string Author { get; private set; }
        public decimal Price { get; private set; }

        public Book() { }
        public Book(string title, string description, int pages, string author, decimal price, Guid id = default)
        {
            Id = id == default ? Guid.NewGuid() : id;
            ValidateDomain(title, description, pages, author, price);
        }

        private void ValidateDomain(string title, string description, int pages, string author, decimal price)
        {
            DomainExceptionValidation.When(string.IsNullOrEmpty(title), "Invalid title. Title is required");
            DomainExceptionValidation.When(string.IsNullOrEmpty(description), "Invalid description. Description is required");
            DomainExceptionValidation.When(pages <= 0, "Invalid pages. Pages must be greater than zero");
            DomainExceptionValidation.When(string.IsNullOrEmpty(author), "Invalid author. Author is required");
            DomainExceptionValidation.When(price <= 0, "Invalid price. Price must be greater than zero");
            Title = title;
            Description = description;
            Pages = pages;
            Author = author;
            Price = price;
        }

        public void UpdateTitle(string title)
        {
            Title = title;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
