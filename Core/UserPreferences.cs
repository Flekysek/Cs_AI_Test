namespace FoodAI.Core
{
    public class UserPreferences
    {
        public string Id { get; set; }
        public List<string> Allergies { get; set; } = [];
        public List<string> FoodPreferences { get; set; } = [];
    }
}
