using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI.backend.Data;
using AI.backend.Models;
using System.Text;

namespace AI.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private NaiveBayesClassifier _sentimentClassifier = new NaiveBayesClassifier();
        private bool _classifierTrained = false;
        private readonly object _trainingLock = new object();

        public ProductsController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // POST: api/products/rate
        [HttpPost("rate")]
        public async Task<ActionResult> RateProduct([FromBody] RatingRequest request)
        {
            var rating = new Rating
            {
                ProductId = request.ProductId,
                UserId = request.UserId,
                RatingValue = request.Rating,
                CreatedAt = DateTime.Now
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[RATING] Rating received: Product {request.ProductId}, User {request.UserId}, Rating: {request.Rating} stars");
            Console.WriteLine("[SUCCESS] Rating saved successfully!");
            Console.ResetColor();

            return Ok(new { message = "Rating saved!" });
        }

        // POST: api/products/comment
        [HttpPost("comment")]
        public async Task<ActionResult> AddComment([FromBody] CommentRequest request)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[COMMENT] Analyzing sentiment for comment: {request.Text}");
                Console.ResetColor();
                
                // Analyze sentiment using ML with fallback
                var sentimentResult = await AnalyzeSentiment(request.Text);
                
                var comment = new Comment
                {
                    ProductId = request.ProductId,
                    UserId = request.UserId,
                    Text = request.Text,
                    Sentiment = sentimentResult.Sentiment,
                    SentimentScore = sentimentResult.Score,
                    CreatedAt = DateTime.Now
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[SUCCESS] Comment saved with sentiment: {sentimentResult.Sentiment} (Score: {sentimentResult.Score:F3})");
                Console.ResetColor();

                return Ok(new { 
                    message = "Comment saved!", 
                    sentiment = sentimentResult.Sentiment,
                    score = sentimentResult.Score,
                    method = sentimentResult.Method
                });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Error adding comment: {ex.Message}");
                Console.ResetColor();
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/products/comments/{productId} - For customers (no sentiment)
        [HttpGet("comments/{productId}")]
        public async Task<ActionResult> GetComments(int productId)
        {
            try
            {
                var comments = await _context.Comments
                    .Where(c => c.ProductId == productId)
                    .Include(c => c.User)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new
                    {
                        id = c.Id,
                        text = c.Text,
                        username = c.User != null ? c.User.Username : "Unknown",
                        createdAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting comments: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/products/comments-with-sentiment/{productId} - For admins only
        [HttpGet("comments-with-sentiment/{productId}")]
        public async Task<ActionResult> GetCommentsWithSentiment(int productId)
        {
            try
            {
                var comments = await _context.Comments
                    .Where(c => c.ProductId == productId)
                    .Include(c => c.User)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new
                    {
                        id = c.Id,
                        text = c.Text,
                        sentiment = c.Sentiment,
                        sentimentScore = c.SentimentScore,
                        username = c.User != null ? c.User.Username : "Unknown",
                        createdAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting comments with sentiment: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ML Model Management Endpoints

        [HttpGet("ml-model/status")]
        public ActionResult GetModelStatus()
        {
            var modelInfo = _sentimentClassifier.GetModelInfo();
            return Ok(new 
            { 
                IsTrained = _classifierTrained,
                ModelType = "Naive Bayes Classifier",
                TrainingExamples = modelInfo["TotalTrainingExamples"],
                VocabularySize = modelInfo["VocabularySize"],
                ClassDistribution = modelInfo["ClassDistribution"],
                WordCounts = modelInfo["WordCounts"]
            });
        }

        [HttpPost("ml-model/retrain")]
        public async Task<ActionResult> RetrainModel()
        {
            try
            {
                _classifierTrained = false;
                await TrainSentimentClassifier();
                
                var modelInfo = _sentimentClassifier.GetModelInfo();
                return Ok(new 
                { 
                    Message = "Model retrained successfully",
                    TrainingExamples = modelInfo["TotalTrainingExamples"],
                    VocabularySize = modelInfo["VocabularySize"],
                    ClassDistribution = modelInfo["ClassDistribution"]
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retraining model: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("ml-model/test")]
        public ActionResult TestModel([FromBody] TestRequest request)
        {
            try
            {
                if (!_classifierTrained)
                    return BadRequest(new { error = "Model not trained" });
                    
                var (sentiment, confidence, probabilities) = _sentimentClassifier.Predict(request.Text);
                
                return Ok(new
                {
                    Text = request.Text,
                    PredictedSentiment = sentiment,
                    Confidence = confidence,
                    Probabilities = probabilities,
                    Method = "Naive Bayes ML"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing model: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // NEW TEST ENDPOINTS FOR ENHANCED LOGGING

        [HttpPost("ml-model/test-training")]
        public async Task<ActionResult> TestTraining()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[TEST] MANUAL TRAINING TEST TRIGGERED VIA API");
                Console.ResetColor();
                
                await TrainSentimentClassifier();
                
                var modelInfo = _sentimentClassifier.GetModelInfo();
                return Ok(new 
                { 
                    Message = "Training test completed successfully",
                    IsTrained = _classifierTrained,
                    TrainingExamples = modelInfo["TotalTrainingExamples"],
                    VocabularySize = modelInfo["VocabularySize"]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("ml-model/predict-test")]
        public ActionResult TestPrediction([FromBody] TestRequest request)
        {
            try
            {
                if (!_classifierTrained)
                    return BadRequest(new { error = "Model not trained yet" });
                    
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[TEST] PREDICTION TEST: '{request.Text}'");
                Console.ResetColor();
                
                var (sentiment, confidence, probabilities) = _sentimentClassifier.Predict(request.Text);
                
                // Detailed logging
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Prediction probabilities:");
                foreach (var prob in probabilities.OrderByDescending(p => p.Value))
                {
                    Console.WriteLine($"   {prob.Key}: {prob.Value:P4}");
                }
                Console.ResetColor();
                
                return Ok(new
                {
                    Text = request.Text,
                    PredictedSentiment = sentiment,
                    Confidence = confidence,
                    Probabilities = probabilities,
                    Method = "Naive Bayes ML"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PRIVATE METHODS

        private async Task<bool> InitializeSentimentClassifier()
        {
            lock (_trainingLock)
            {
                if (_classifierTrained) return true;
                
                try
                {
                    // Try to load existing model first
                    _sentimentClassifier.LoadModel();
                    
                    // If model has reasonable training data, use it
                    var modelInfo = _sentimentClassifier.GetModelInfo();
                    var totalExamples = (int)(modelInfo["TotalTrainingExamples"] ?? 0);
                    
                    if (totalExamples >= 20) // Minimum training examples
                    {
                        _classifierTrained = true;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[SUCCESS] Using pre-trained model with {totalExamples} examples");
                        Console.ResetColor();
                        return true;
                    }
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[INFO] Pre-trained model insufficient, training new model...");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] Error loading model: {ex.Message}");
                    Console.ResetColor();
                }
                
                return false;
            }
        }

        private async Task TrainSentimentClassifier()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=== STARTING SENTIMENT CLASSIFIER TRAINING ===");
                Console.ResetColor();
                
                var trainingData = await GetTrainingData();
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[INFO] Total training data gathered: {trainingData.Count} examples");
                Console.ResetColor();
                
                if (trainingData.Count < 10)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[WARNING] Insufficient training data - Need at least 10 examples");
                    Console.WriteLine("[WARNING] Using rule-based approach instead");
                    Console.ResetColor();
                    return;
                }
                
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("[INFO] Starting Naive Bayes training...");
                Console.ResetColor();
                
                _sentimentClassifier.Train(trainingData);
                _classifierTrained = true;
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[SUCCESS] Sentiment classifier training completed");
                Console.ResetColor();
                
                // Test the model immediately with some examples
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[TEST] Testing trained model with sample texts:");
                Console.ResetColor();
                
                var testTexts = new[]
                {
                    "This product is absolutely excellent and perfect!",
                    "Good quality and works well",
                    "It's okay, nothing special",
                    "Poor quality and disappointing",
                    "Terrible product, broken and useless"
                };
                
                foreach (var testText in testTexts)
                {
                    try
                    {
                        var (sentiment, confidence, probabilities) = _sentimentClassifier.Predict(testText);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"   Test: '{testText}' -> {sentiment} ({confidence:P1} confidence)");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"   Test failed: {ex.Message}");
                        Console.ResetColor();
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=== TRAINING COMPLETE ===");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Error training sentiment classifier: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                Console.ResetColor();
            }
        }

        private async Task<List<SentimentTrainingData>> GetTrainingData()
        {
            var trainingData = new List<SentimentTrainingData>();
            
            try
            {
                // Use existing comments as training data
                var existingComments = await _context.Comments
                    .Where(c => c.Text != null && c.Sentiment != null && c.Text.Length > 10)
                    .Select(c => new SentimentTrainingData 
                    { 
                        Text = c.Text, 
                        Sentiment = c.Sentiment 
                    })
                    .ToListAsync();

                trainingData.AddRange(existingComments);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[INFO] Loaded {existingComments.Count} existing comments for training");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Error loading existing comments: {ex.Message}");
                Console.ResetColor();
            }
            
            // Add synthetic data if we don't have enough
            if (trainingData.Count < 50)
            {
                var syntheticData = GetComprehensiveTrainingData();
                trainingData.AddRange(syntheticData);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[INFO] Added {syntheticData.Count} synthetic training examples");
                Console.ResetColor();
            }
            
            // Ensure balanced classes
            trainingData = BalanceTrainingData(trainingData);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[INFO] Final training dataset: {trainingData.Count} examples");
            Console.ResetColor();
            
            return trainingData;
        }

        private List<SentimentTrainingData> GetComprehensiveTrainingData()
        {
            return new List<SentimentTrainingData>
            {
                // Very Positive - Strong positive emotions and superlatives
                new() { Text = "This product is absolutely excellent and perfect in every way", Sentiment = "Very Positive" },
                new() { Text = "I love this amazing fantastic brilliant outstanding product", Sentiment = "Very Positive" },
                new() { Text = "Flawless performance phenomenal quality stunning design", Sentiment = "Very Positive" },
                new() { Text = "Exceptional value superb craftsmanship perfect functionality", Sentiment = "Very Positive" },
                new() { Text = "Revolutionary technology breathtaking innovation masterpiece", Sentiment = "Very Positive" },
                
                // Positive - Good but not exceptional
                new() { Text = "Good product works well reliable performance nice quality", Sentiment = "Positive" },
                new() { Text = "Great value decent build satisfactory experience", Sentiment = "Positive" },
                new() { Text = "Solid construction comfortable use efficient operation", Sentiment = "Positive" },
                new() { Text = "Works fine adequate performance reasonable price", Sentiment = "Positive" },
                new() { Text = "Nice features good design acceptable quality", Sentiment = "Positive" },
                
                // Neutral - Mixed or indifferent
                new() { Text = "Average product okay performance nothing special", Sentiment = "Neutral" },
                new() { Text = "Mediocre quality ordinary features standard performance", Sentiment = "Neutral" },
                new() { Text = "Neither good nor bad acceptable but unimpressive", Sentiment = "Neutral" },
                new() { Text = "Basic functionality adequate for needs average", Sentiment = "Neutral" },
                new() { Text = "It works fine but nothing extraordinary mediocre", Sentiment = "Neutral" },
                
                // Negative - Clearly negative but not extreme
                new() { Text = "Poor quality disappointing performance bad design", Sentiment = "Negative" },
                new() { Text = "Slow performance unreliable operation disappointing", Sentiment = "Negative" },
                new() { Text = "Cheap build flimsy construction poor value", Sentiment = "Negative" },
                new() { Text = "Underwhelming performance mediocre quality bad", Sentiment = "Negative" },
                new() { Text = "Frustrating experience problematic functionality poor", Sentiment = "Negative" },
                
                // Very Negative - Extreme negative emotions
                new() { Text = "Terrible product awful quality broken defective garbage", Sentiment = "Very Negative" },
                new() { Text = "Horrible experience worst product useless trash", Sentiment = "Very Negative" },
                new() { Text = "Complete waste of money junk terrible awful", Sentiment = "Very Negative" },
                new() { Text = "Broken defective faulty ruined disaster horrible", Sentiment = "Very Negative" },
                new() { Text = "Worst purchase ever garbage trash terrible defective", Sentiment = "Very Negative" }
            };
        }

        private List<SentimentTrainingData> BalanceTrainingData(List<SentimentTrainingData> data)
        {
            var grouped = data.GroupBy(d => d.Sentiment)
                             .ToDictionary(g => g.Key, g => g.ToList());
            
            var maxCount = grouped.Values.Max(g => g.Count);
            var balancedData = new List<SentimentTrainingData>();
            
            foreach (var sentiment in new[] { "Very Positive", "Positive", "Neutral", "Negative", "Very Negative" })
            {
                if (grouped.ContainsKey(sentiment))
                {
                    var sentimentData = grouped[sentiment];
                    // Oversample if needed, but limit to avoid too much duplication
                    while (sentimentData.Count < Math.Min(maxCount, 20))
                    {
                        sentimentData.AddRange(sentimentData.Take(5));
                    }
                    balancedData.AddRange(sentimentData.Take(maxCount));
                }
            }
            
            return balancedData;
        }

        private async Task<SentimentResult> AnalyzeSentiment(string text)
        {
            // Try ML first
            var mlResult = await AnalyzeSentimentML(text);
            if (mlResult.Score > 0.6) // Good confidence
            {
                return mlResult;
            }
            
            // Fall back to rule-based
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[INFO] ML confidence low, falling back to rule-based analysis");
            Console.ResetColor();
            return await AnalyzeSentimentRuleBased(text);
        }

        private async Task<SentimentResult> AnalyzeSentimentML(string text)
        {
            try
            {
                if (!_classifierTrained && !await InitializeSentimentClassifier())
                {
                    await TrainSentimentClassifier();
                }
                
                if (_classifierTrained)
                {
                    var (sentiment, confidence, probabilities) = _sentimentClassifier.Predict(text);
                    
                    // Log detailed probabilities for debugging
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Prediction probabilities:");
                    foreach (var prob in probabilities.OrderByDescending(p => p.Value))
                    {
                        Console.WriteLine($"   {prob.Key}: {prob.Value:P2}");
                    }
                    Console.ResetColor();
                    
                    return new SentimentResult { 
                        Sentiment = sentiment, 
                        Score = confidence,
                        Method = "Naive Bayes ML"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] ML prediction failed: {ex.Message}");
                Console.ResetColor();
            }
            
            return new SentimentResult { Score = 0 }; // Indicate ML failed
        }

        private async Task<SentimentResult> AnalyzeSentimentRuleBased(string text)
        {
            // Enhanced technology-specific sentiment analysis
            var veryPositiveWords = new[] { 
                "excellent", "perfect", "best", "love", "fantastic", "outstanding", "amazing",
                "awesome", "superb", "brilliant", "exceptional", "flawless", "phenomenal",
                "masterpiece", "revolutionary", "gamechanger", "breathtaking", "stunning"
            };
            
            var positiveWords = new[] { 
                "good", "great", "nice", "wonderful", "quality", "reliable", "durable",
                "comfortable", "premium", "responsive", "crisp", "bright", "clear",
                "powerful", "efficient", "stable", "quick", "easy", "simple", "intuitive",
                "worth", "value", "recommend", "impressive", "sleek", "lightweight", "compact",
                "smooth", "fast", "solid", "well", "fine", "decent", "adequate", "satisfactory"
            };
            
            var negativeWords = new[] { 
                "bad", "poor", "disappointing", "boring", "useless", "slow", "lag", "laggy",
                "unreliable", "cheap", "flimsy", "noisy", "loud", "heavy", "bulky",
                "expensive", "overpriced", "difficult", "complicated", "confusing",
                "mediocre", "average", "ordinary", "unimpressive", "underwhelming"
            };
            
            var veryNegativeWords = new[] { 
                "terrible", "awful", "horrible", "worst", "hate", "dislike", "waste", "broken",
                "broke", "stopped", "crashed", "froze", "freeze", "frozen", "defective",
                "faulty", "damaged", "scratched", "cracked", "bent", "dead", "failed",
                "failure", "error", "bug", "buggy", "glitch", "problem", "issue", "junk",
                "garbage", "trash", "return", "refund", "unacceptable", "disgusting"
            };

            var words = text.ToLower().Split(' ', '.', ',', '!', '?', ';', ':');
            
            var veryPositiveCount = words.Count(word => veryPositiveWords.Contains(word));
            var positiveCount = words.Count(word => positiveWords.Contains(word));
            var negativeCount = words.Count(word => negativeWords.Contains(word));
            var veryNegativeCount = words.Count(word => veryNegativeWords.Contains(word));

            // Calculate weighted sentiment score
            var totalPositive = (veryPositiveCount * 2) + positiveCount;
            var totalNegative = (veryNegativeCount * 2) + negativeCount;
            var totalSentimentWords = totalPositive + totalNegative;
            
            double score = 0.5; // neutral default

            if (totalSentimentWords > 0)
            {
                score = (double)totalPositive / totalSentimentWords;
                
                // Additional scoring adjustments
                if (text.ToLower().Contains("excellent") || text.ToLower().Contains("perfect") || 
                    text.ToLower().Contains("best") || text.ToLower().Contains("love"))
                {
                    score = Math.Min(score + 0.15, 1.0);
                }
                
                if (text.ToLower().Contains("broken") || text.ToLower().Contains("broke") || 
                    text.ToLower().Contains("stopped working") || text.ToLower().Contains("defective"))
                {
                    score = Math.Max(score - 0.25, 0.0);
                }
                
                if (text.ToLower().Contains("refund") || text.ToLower().Contains("return"))
                {
                    score = Math.Max(score - 0.2, 0.0);
                }
            }

            // Enhanced sentiment categorization with 5 levels
            string sentiment;
            if (score >= 0.8) sentiment = "Very Positive";
            else if (score >= 0.6) sentiment = "Positive";
            else if (score >= 0.4) sentiment = "Neutral";
            else if (score >= 0.2) sentiment = "Negative";
            else sentiment = "Very Negative";

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Rule-based analysis: '{text}' -> {sentiment} (Score: {score})");
            Console.WriteLine($"Very Positive: {veryPositiveCount}, Positive: {positiveCount}, Negative: {negativeCount}, Very Negative: {veryNegativeCount}");
            Console.ResetColor();

            return new SentimentResult { 
                Sentiment = sentiment, 
                Score = score,
                Method = "Rule-Based"
            };
        }
    }

    // Request/Response Classes

    public class RatingRequest
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
    }

    public class CommentRequest
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class SentimentResult
    {
        public string Sentiment { get; set; } = "Neutral";
        public double Score { get; set; }
        public string Method { get; set; } = "RuleBased";
    }

    public class TestRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}