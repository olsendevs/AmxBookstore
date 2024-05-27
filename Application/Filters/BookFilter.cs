namespace AmxBookstore.Application.Filters
{
    public class BookFilter
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinPages { get; set; }
        public int? MaxPages { get; set; }
    }
}
