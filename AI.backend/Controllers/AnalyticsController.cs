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

        [HttpGet("comment-analysis")]
        public async Task<ActionResult> GetCommentAnalysis()
        {
            try
            {
                // Use safe query with null handling
                var comments = await _context.Comments
                    .Include(c => c.Product)
                    .Include(c => c.User)
                    .Where(c => c.Text != null && c.Sentiment != null) // Filter out null values
                    .ToListAsync();

                if (!comments.Any())
                {
                    return Ok(new List<object>());
                }

                var analysis = comments.GroupBy(c => c.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        ProductName = g.First().Product?.Name ?? "Unknown",
                        TotalComments = g.Count(),
                        PositiveComments = g.Count(c => c.Sentiment == "Positive"),
                        NegativeComments = g.Count(c => c.Sentiment == "Negative"),
                        NeutralComments = g.Count(c => c.Sentiment == "Neutral"),
                        AverageSentimentScore = g.Average(c => c.SentimentScore),
                        OverallSentiment = g.Average(c => c.SentimentScore) > 0.6 ? "Positive" : 
                                         g.Average(c => c.SentimentScore) < 0.4 ? "Negative" : "Neutral",
                        RecentComments = g.OrderByDescending(c => c.CreatedAt)
                                         .Take(5)
                                         .Select(c => new
                                         {
                                             Id = c.Id,
                                             Text = c.Text ?? string.Empty,
                                             Sentiment = c.Sentiment ?? "Neutral",
                                             SentimentScore = c.SentimentScore,
                                             Username = c.User?.Username ?? "Unknown",
                                             CreatedAt = c.CreatedAt
                                         })
                                         .ToList(),
                        CommentTrend = new
                        {
                            Last7Days = g.Count(c => c.CreatedAt >= DateTime.Now.AddDays(-7)),
                            Last30Days = g.Count(c => c.CreatedAt >= DateTime.Now.AddDays(-30))
                        }
                    })
                    .ToList();

                return Ok(analysis);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in comment analysis: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("detailed-comment-analysis")]
        public async Task<ActionResult> GetDetailedCommentAnalysis()
        {
            try
            {
                var comments = await _context.Comments
                    .Include(c => c.Product)
                    .Include(c => c.User)
                    .Where(c => c.Text != null && c.Sentiment != null)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                // Update overall stats to include new categories
                var overallStats = new
                {
                    TotalComments = comments.Count,
                    VeryPositiveComments = comments.Count(c => c.Sentiment == "Very Positive"),
                    PositiveComments = comments.Count(c => c.Sentiment == "Positive"),
                    NeutralComments = comments.Count(c => c.Sentiment == "Neutral"),
                    NegativeComments = comments.Count(c => c.Sentiment == "Negative"),
                    VeryNegativeComments = comments.Count(c => c.Sentiment == "Very Negative"),
                    AverageSentimentScore = comments.Any() ? Math.Round(comments.Average(c => c.SentimentScore), 2) : 0,
                    CommentsLast7Days = comments.Count(c => c.CreatedAt >= DateTime.Now.AddDays(-7)),
                    CommentsLast30Days = comments.Count(c => c.CreatedAt >= DateTime.Now.AddDays(-30))
                };

                var userCommentStats = comments
                    .GroupBy(c => c.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        Username = g.First().User?.Username ?? "Unknown",
                        TotalComments = g.Count(),
                        PositiveComments = g.Count(c => c.Sentiment == "Positive"),
                        NegativeComments = g.Count(c => c.Sentiment == "Negative"),
                        AverageSentimentScore = g.Average(c => c.SentimentScore)
                    })
                    .OrderByDescending(u => u.TotalComments)
                    .Take(10)
                    .ToList();

                var recentComments = comments
                    .Take(20)
                    .Select(c => new
                    {
                        Id = c.Id,
                        ProductName = c.Product?.Name ?? "Unknown",
                        Text = (c.Text ?? string.Empty).Length > 100 ? 
                              (c.Text ?? string.Empty).Substring(0, 100) + "..." : 
                              (c.Text ?? string.Empty),
                        FullText = c.Text ?? string.Empty,
                        Sentiment = c.Sentiment ?? "Neutral",
                        SentimentScore = c.SentimentScore,
                        Username = c.User?.Username ?? "Unknown",
                        CreatedAt = c.CreatedAt,
                        TimeAgo = GetTimeAgo(c.CreatedAt)
                    })
                    .ToList();

                var productAnalysis = comments.GroupBy(c => c.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        ProductName = g.First().Product?.Name ?? "Unknown Product",
                        TotalComments = g.Count(),
                        VeryPositiveComments = g.Count(c => c.Sentiment == "Very Positive"),
                        PositiveComments = g.Count(c => c.Sentiment == "Positive"),
                        NeutralComments = g.Count(c => c.Sentiment == "Neutral"),
                        NegativeComments = g.Count(c => c.Sentiment == "Negative"),
                        VeryNegativeComments = g.Count(c => c.Sentiment == "Very Negative"),
                        AverageSentimentScore = Math.Round(g.Average(c => c.SentimentScore), 2),
                        OverallSentiment = g.Average(c => c.SentimentScore) > 0.7 ? "Very Positive" :
                                        g.Average(c => c.SentimentScore) > 0.55 ? "Positive" :
                                        g.Average(c => c.SentimentScore) > 0.45 ? "Neutral" :
                                        g.Average(c => c.SentimentScore) > 0.3 ? "Negative" : "Very Negative"
                    })
                    .OrderByDescending(p => p.TotalComments)
                    .ToList();

                var result = new
                {
                    OverallStats = overallStats,
                    TopCommenters = userCommentStats,
                    RecentComments = recentComments,
                    ProductAnalysis = productAnalysis
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in detailed comment analysis: {ex.Message}");
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

        private string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.Now - date;
            
            if (timeSpan.TotalMinutes < 1) return "just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays}d ago";
            
            return $"{(int)(timeSpan.TotalDays / 30)}mo ago";
        }
    }
}