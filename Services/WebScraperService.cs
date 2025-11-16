using AngleSharp;
using FoodAI.Core;

namespace FoodAI.Services
{
    public interface IWebScraperService
    {
        Task<List<FoodItem>> ScrapeFoodItemsAsync(string url);
    }

    public class WebScraperService : IWebScraperService
    {
        private readonly HttpClient _httpClient;

        public WebScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FoodItem>> ScrapeFoodItemsAsync(string url)
        {
            var foodItems = new List<FoodItem>();
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            try
            {
                var document = await context.OpenAsync(url);

                foreach (var element in document.QuerySelectorAll("p"))
                {
                    foodItems.Add(new FoodItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = element.TextContent.Length > 50 ? element.TextContent.Substring(0, 50) + "..." : element.TextContent,
                        Description = element.TextContent,
                        Price = 0.0m,
                        SourceUrl = url,
                        ScrapedDate = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping {url}: {ex.Message}");
            }

            return foodItems;
        }
    }
}
