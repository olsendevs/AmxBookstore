namespace AmxBookstore.Application.DTOs
{
    public class BookDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Pages { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
    }
}
