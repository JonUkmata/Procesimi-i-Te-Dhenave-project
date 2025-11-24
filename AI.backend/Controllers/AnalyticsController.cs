using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI.backend.Data;
using AI.backend.Models;
using System.Text.Json;

namespace AI.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("ai-analysis")]
        public async Task<ActionResult> GetAIAnalysis()
        {
            try
            {
                Console.WriteLine("STARTING AI ANALYSIS DEBUG");

                // Get all products and ratings with proper includes
                var products = await _context.Products.ToListAsync();
                var allRatings = await _context.Ratings
                    .Include(r => r.User)
                    .ToListAsync();

                Console.WriteLine($"PRODUCTS: {products.Count}");
                foreach (var p in products)
                {
                    Console.WriteLine($"   - {p.Id}: {p.Name}");
                }

                Console.WriteLine($"RATINGS: {allRatings.Count}");
                foreach (var r in allRatings)
                {
                    Console.WriteLine($"   - Product {r.ProductId}: {r.RatingValue} stars by User {r.UserId}");
                }

                // Create analysis for each product
                var analysis = new List<object>();
                foreach (var product in products)
                {
                    var productRatings = allRatings.Where(r => r.ProductId == product.Id).ToList();
                    Console.WriteLine($"Analyzing {product.Name}: {productRatings.Count} ratings");

                    var productAnalysis = AnalyzeProduct(product, productRatings);
                    analysis.Add(productAnalysis);

                    // Debug the final output
                    Console.WriteLine($"FINAL ANALYSIS for {product.Name}:");
                    Console.WriteLine($"   Average: {productAnalysis.AverageRating}");
                    Console.WriteLine($"   Total: {productAnalysis.TotalRatings}");
                    Console.WriteLine($"   Category: {productAnalysis.Category}");
                    Console.WriteLine($"   Distribution: {JsonSerializer.Serialize(productAnalysis.RatingDistribution)}");
                    Console.WriteLine("---");
                }

                var insights = GenerateBusinessInsights(analysis);

                Console.WriteLine("AI ANALYSIS COMPLETE - SENDING TO FRONTEND");
                var result = new { 
                    Products = analysis,
                    Insights = insights
                };
                
                Console.WriteLine($"FINAL JSON BEING SENT: {JsonSerializer.Serialize(result)}");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRITICAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("product-ratings")]
        public async Task<ActionResult> GetProductRatings()
        {
            try
            {
                var productsWithRatings = await _context.Products
                    .Include(p => p.Ratings)
                    .ThenInclude(r => r.User)
                    .Select(p => new
                    {
                        productId = p.Id,
                        productName = p.Name,
                        averageRating = p.Ratings.Any() ? p.Ratings.Average(r => r.RatingValue) : 0,
                        totalRatings = p.Ratings.Count,
                        ratings = p.Ratings.Select(r => new
                        {
                            rating = r.RatingValue,
                            username = r.User != null ? r.User.Username : "Unknown",
                            date = r.CreatedAt
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(productsWithRatings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProductRatings: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private dynamic AnalyzeProduct(Product product, List<Rating> productRatings)
        {
            // Safely handle null or empty ratings
            var ratings = productRatings?.Where(r => r != null).Select(r => r.RatingValue).ToList() ?? new List<int>();
            var averageRating = ratings.Any() ? ratings.Average() : 0;
            var totalRatings = ratings.Count;

            // Create rating distribution - SIMPLIFIED AND GUARANTEED TO WORK
            var ratingDistribution = new Dictionary<string, int>
            {
                { "1", 0 },
                { "2", 0 }, 
                { "3", 0 },
                { "4", 0 },
                { "5", 0 }
            };

            foreach (var rating in ratings)
            {
                var key = rating.ToString();
                if (ratingDistribution.ContainsKey(key))
                {
                    ratingDistribution[key]++;
                }
            }

            var category = totalRatings == 0 ? "No Ratings" :
                          averageRating >= 4.5 ? "Excellent" :
                          averageRating >= 4.0 ? "Very Good" :
                          averageRating >= 3.0 ? "Average" :
                          averageRating >= 2.0 ? "Needs Improvement" : "Critical";

            var recommendation = category switch
            {
                "Excellent" => "Promote this product!",
                "Very Good" => "Strong performer",
                "Average" => "Monitor closely",
                "Needs Improvement" => "Investigate issues",
                "Critical" => "Urgent action required!",
                _ => "No customer feedback yet"
            };

            return new
            {
                ProductId = product.Id,
                ProductName = product.Name,
                AverageRating = Math.Round(averageRating, 2),
                TotalRatings = totalRatings,
                Category = category,
                Recommendation = recommendation,
                Sentiment = "Positive",
                RatingDistribution = ratingDistribution,
                RiskLevel = "Low"
            };
        }

        private object GenerateBusinessInsights(List<object> products)
        {
            var productList = products.Cast<dynamic>().ToList();
            var productsWithRatings = productList.Where(p => p.TotalRatings > 0).ToList();
            
            var overallSatisfaction = productsWithRatings.Any() 
                ? Math.Round(productsWithRatings.Average(p => (double)p.AverageRating), 2)
                : 0;

            return new
            {
                TotalProductsAnalyzed = productList.Count,
                ProductsWithRatings = productsWithRatings.Count,
                ExcellentProducts = productList.Count(p => p.Category == "Excellent"),
                CriticalProducts = productList.Count(p => p.Category == "Critical"),
                OverallCustomerSatisfaction = overallSatisfaction,
                KeyInsight = productsWithRatings.Any() ? "Product analysis complete" : "No ratings data available",
                Recommendation = productList.Any(p => p.Category == "Critical") 
                    ? "Review critical products for improvements" 
                    : "Continue current strategy"
            };
        }
    }
}