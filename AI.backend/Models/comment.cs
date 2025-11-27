namespace AI.backend.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Sentiment { get; set; } = "Neutral"; // Positive, Negative, Neutral
        public double SentimentScore { get; set; } // 0-1 score
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public Product? Product { get; set; }
        public User? User { get; set; }
    }
}