namespace AI.backend.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        
        // Navigation property
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}