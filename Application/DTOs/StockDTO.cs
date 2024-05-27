namespace AmxBookstore.Application.DTOs
{
    public class StockDTO
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public int Quantity { get; set; }
    }
}
