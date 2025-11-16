namespace FoodAI.Core
{
    public class ChatMessage
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; } 
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
