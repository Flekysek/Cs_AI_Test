using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FoodAI.Core;
using FoodAI.Services;
using FoodAI.Data;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace FoodAI.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebScraperService _webScraperService;
        private readonly MongoDBService _mongoDbService;
        private readonly IGeminiAIService _geminiAiService;
        private readonly IMemoryCache _cache;

        [BindProperty]
        public string UrlsInput { get; set; }

        public List<FoodItem> FoodItems { get; set; } = [];
        public List<ChatMessage> ChatMessages { get; set; } = [];

        public IndexModel(ILogger<IndexModel> logger, IWebScraperService webScraperService, MongoDBService mongoDbService, IGeminiAIService geminiAiService, IMemoryCache cache)
        {
            _logger = logger;
            _webScraperService = webScraperService;
            _mongoDbService = mongoDbService;
            _geminiAiService = geminiAiService;
            _cache = cache;
        }

        public async Task OnGetAsync()
        {
            ChatMessages = await _mongoDbService.ChatMessages.Find(c => true).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrWhiteSpace(UrlsInput))
            {
                var urls = UrlsInput.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(u => u.Trim())
                                    .ToList();

                foreach (var url in urls)
                {
                    var cacheKey = $"FoodItems_{url}_{DateTime.Today:yyyyMMdd}";
                    if (!_cache.TryGetValue(cacheKey, out List<FoodItem> cachedFoodItems))
                    {
                        var today = DateTime.Today;
                        var filter = Builders<FoodItem>.Filter.And(
                            Builders<FoodItem>.Filter.Eq(f => f.SourceUrl, url),
                            Builders<FoodItem>.Filter.Gte(f => f.ScrapedDate, today),
                            Builders<FoodItem>.Filter.Lt(f => f.ScrapedDate, today.AddDays(1))
                        );
                        cachedFoodItems = await _mongoDbService.FoodItems.Find(filter).ToListAsync();

                        if (cachedFoodItems == null || !cachedFoodItems.Any())
                        {
                            var scrapedItems = await _webScraperService.ScrapeFoodItemsAsync(url);
                            if (scrapedItems != null && scrapedItems.Any())
                            {
                                await _mongoDbService.FoodItems.InsertManyAsync(scrapedItems);
                                cachedFoodItems = scrapedItems;
                            }
                        }

                        _cache.Set(cacheKey, cachedFoodItems, TimeSpan.FromHours(1)); // Cache for 1 hour
                    }
                    FoodItems.AddRange(cachedFoodItems);
                }
            }

            ChatMessages = await _mongoDbService.ChatMessages.Find(c => true).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostChatAsync(string chatInput)
        {
            if (!string.IsNullOrWhiteSpace(chatInput))
            {
                var userMessage = new ChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = "user123",
                    Role = "user",
                    Content = chatInput,
                    Timestamp = DateTime.UtcNow
                };
                await _mongoDbService.ChatMessages.InsertOneAsync(userMessage);

                var userPreferences = await _mongoDbService.UserPreferences.Find(p => p.Id == "user123").FirstOrDefaultAsync();
                if (userPreferences == null)
                {
                    userPreferences = new UserPreferences { Id = "user123" };
                    await _mongoDbService.UserPreferences.InsertOneAsync(userPreferences);
                }

                var promptBuilder = new System.Text.StringBuilder();
                promptBuilder.AppendLine("You are a helpful AI assistant that provides information about food items and prices.");
                promptBuilder.AppendLine("Consider the following user preferences:");
                promptBuilder.AppendLine($"Allergies: {string.Join(", ", userPreferences.Allergies)}");
                promptBuilder.AppendLine($"Food Preferences: {string.Join(", ", userPreferences.FoodPreferences)}");
                promptBuilder.AppendLine("Here are the currently scraped food items:");
                foreach (var item in FoodItems)
                {
                    promptBuilder.AppendLine($"- Name: {item.Name}, Description: {item.Description}, Price: {item.Price:C}");
                }
                promptBuilder.AppendLine($"User query: {chatInput}");

                var aiResponseContent = await _geminiAiService.GenerateContentAsync(promptBuilder.ToString());

                var aiMessage = new ChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = "user123",
                    Role = "ai",
                    Content = aiResponseContent,
                    Timestamp = DateTime.UtcNow
                };
                await _mongoDbService.ChatMessages.InsertOneAsync(aiMessage);
            }

            ChatMessages = await _mongoDbService.ChatMessages.Find(c => true).ToListAsync();
            await OnPostAsync(); 
            return Page();
        }
    }
}
