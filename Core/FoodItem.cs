namespace FoodAI.Core
{
    public class FoodItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string SourceUrl { get; set; }
        public DateTime ScrapedDate { get; set; }
    }
}
