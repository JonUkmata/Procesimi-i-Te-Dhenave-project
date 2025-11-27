using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI.backend.Models
{
    public class SentimentTrainingData
    {
        public string Text { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
    }

    public class NaiveBayesModelData
    {
        public Dictionary<string, int> VeryPositiveWordCounts { get; set; } = new();
        public Dictionary<string, int> PositiveWordCounts { get; set; } = new();
        public Dictionary<string, int> NeutralWordCounts { get; set; } = new();
        public Dictionary<string, int> NegativeWordCounts { get; set; } = new();
        public Dictionary<string, int> VeryNegativeWordCounts { get; set; } = new();
        
        public int VeryPositiveTotalWords { get; set; }
        public int PositiveTotalWords { get; set; }
        public int NeutralTotalWords { get; set; }
        public int NegativeTotalWords { get; set; }
        public int VeryNegativeTotalWords { get; set; }
        
        public int VeryPositiveDocs { get; set; }
        public int PositiveDocs { get; set; }
        public int NeutralDocs { get; set; }
        public int NegativeDocs { get; set; }
        public int VeryNegativeDocs { get; set; }
        
        public int TotalDocs { get; set; }
    }

    public class NaiveBayesClassifier
    {
        private NaiveBayesModelData _model = new();
        private double _alpha = 1.0; // Laplace smoothing
        private readonly string _modelPath = "sentiment-model.json";
        
        public void Train(List<SentimentTrainingData> trainingData)
        {
            if (trainingData == null || trainingData.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] No training data provided!");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== STARTING NAIVE BAYES TRAINING ===");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[INFO] Training dataset size: {trainingData.Count} examples");
            Console.ResetColor();
            
            // Show class distribution before training
            var initialDistribution = trainingData.GroupBy(t => t.Sentiment)
                                         .ToDictionary(g => g.Key, g => g.Count());
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Initial class distribution:");
            foreach (var dist in initialDistribution)
            {
                Console.WriteLine($"   {dist.Key}: {dist.Value} examples");
            }
            Console.ResetColor();
            
            // Reset model
            _model = new NaiveBayesModelData();
            
            int processed = 0;
            foreach (var data in trainingData)
            {
                processed++;
                if (processed % 10 == 0) // Log progress every 10 examples
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[PROGRESS] Processing training example {processed}/{trainingData.Count}");
                    Console.ResetColor();
                }
                
                var words = Tokenize(data.Text);
                _model.TotalDocs++;
                
                // Log some examples being processed
                if (processed <= 5)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"   Example {processed}: '{data.Text.Substring(0, Math.Min(50, data.Text.Length))}...' -> {data.Sentiment}");
                    Console.WriteLine($"   Extracted {words.Count} words: [{string.Join(", ", words.Take(5))}...]");
                    Console.ResetColor();
                }
                
                switch (data.Sentiment)
                {
                    case "Very Positive":
                        _model.VeryPositiveDocs++;
                        UpdateWordCounts(words, _model.VeryPositiveWordCounts, "Very Positive");
                        break;
                    case "Positive":
                        _model.PositiveDocs++;
                        UpdateWordCounts(words, _model.PositiveWordCounts, "Positive");
                        break;
                    case "Neutral":
                        _model.NeutralDocs++;
                        UpdateWordCounts(words, _model.NeutralWordCounts, "Neutral");
                        break;
                    case "Negative":
                        _model.NegativeDocs++;
                        UpdateWordCounts(words, _model.NegativeWordCounts, "Negative");
                        break;
                    case "Very Negative":
                        _model.VeryNegativeDocs++;
                        UpdateWordCounts(words, _model.VeryNegativeWordCounts, "Very Negative");
                        break;
                }
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=== TRAINING COMPLETED ===");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Final class distribution:");
            Console.WriteLine($"   Very Positive: {_model.VeryPositiveDocs} documents, {_model.VeryPositiveTotalWords} words");
            Console.WriteLine($"   Positive: {_model.PositiveDocs} documents, {_model.PositiveTotalWords} words");
            Console.WriteLine($"   Neutral: {_model.NeutralDocs} documents, {_model.NeutralTotalWords} words");
            Console.WriteLine($"   Negative: {_model.NegativeDocs} documents, {_model.NegativeTotalWords} words");
            Console.WriteLine($"   Very Negative: {_model.VeryNegativeDocs} documents, {_model.VeryNegativeTotalWords} words");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[INFO] Vocabulary size: {GetVocabularySize()} unique words");
            Console.ResetColor();
            
            // Show some learned words for each class
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Top words learned for each class:");
            Console.ResetColor();
            ShowTopWordsByClass(5);
            
            SaveModel();
        }
        
        private void UpdateWordCounts(List<string> words, Dictionary<string, int> wordCounts, string sentiment)
        {
            foreach (var word in words)
            {
                wordCounts[word] = wordCounts.GetValueOrDefault(word, 0) + 1;
                
                // Update the total words count for the specific sentiment
                switch (sentiment)
                {
                    case "Very Positive":
                        _model.VeryPositiveTotalWords++;
                        break;
                    case "Positive":
                        _model.PositiveTotalWords++;
                        break;
                    case "Neutral":
                        _model.NeutralTotalWords++;
                        break;
                    case "Negative":
                        _model.NegativeTotalWords++;
                        break;
                    case "Very Negative":
                        _model.VeryNegativeTotalWords++;
                        break;
                }
            }
        }
        
        public (string Sentiment, double Score, Dictionary<string, double> Probabilities) Predict(string text)
        {
            if (_model.TotalDocs == 0)
                throw new InvalidOperationException("Classifier not trained");
                
            var words = Tokenize(text);
            var vocabSize = GetVocabularySize();
            
            // Prior probabilities (log)
            var logPriors = new Dictionary<string, double>
            {
                ["Very Positive"] = Math.Log((double)_model.VeryPositiveDocs / _model.TotalDocs),
                ["Positive"] = Math.Log((double)_model.PositiveDocs / _model.TotalDocs),
                ["Neutral"] = Math.Log((double)_model.NeutralDocs / _model.TotalDocs),
                ["Negative"] = Math.Log((double)_model.NegativeDocs / _model.TotalDocs),
                ["Very Negative"] = Math.Log((double)_model.VeryNegativeDocs / _model.TotalDocs)
            };
            
            // Calculate likelihood for each word
            foreach (var word in words)
            {
                logPriors["Very Positive"] += Math.Log(GetWordProbability(word, _model.VeryPositiveWordCounts, _model.VeryPositiveTotalWords, vocabSize));
                logPriors["Positive"] += Math.Log(GetWordProbability(word, _model.PositiveWordCounts, _model.PositiveTotalWords, vocabSize));
                logPriors["Neutral"] += Math.Log(GetWordProbability(word, _model.NeutralWordCounts, _model.NeutralTotalWords, vocabSize));
                logPriors["Negative"] += Math.Log(GetWordProbability(word, _model.NegativeWordCounts, _model.NegativeTotalWords, vocabSize));
                logPriors["Very Negative"] += Math.Log(GetWordProbability(word, _model.VeryNegativeWordCounts, _model.VeryNegativeTotalWords, vocabSize));
            }
            
            // Convert to probabilities using softmax
            var (normalizedProbs, predictedSentiment, confidence) = ApplySoftmax(logPriors);
            
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[PREDICTION] Naive Bayes Prediction: '{text}' -> {predictedSentiment} (Confidence: {confidence:P2})");
            Console.ResetColor();
            
            return (predictedSentiment, confidence, normalizedProbs);
        }
        
        private (Dictionary<string, double> NormalizedProbs, string Predicted, double Confidence) ApplySoftmax(Dictionary<string, double> logProbabilities)
        {
            var maxLogProb = logProbabilities.Values.Max();
            var expSum = logProbabilities.Values.Sum(logProb => Math.Exp(logProb - maxLogProb));
            
            var normalizedProbs = logProbabilities.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Exp(kvp.Value - maxLogProb) / expSum
            );
            
            var predicted = normalizedProbs.Aggregate((x, y) => x.Value > y.Value ? x : y);
            return (normalizedProbs, predicted.Key, predicted.Value);
        }
        
        private double GetWordProbability(string word, Dictionary<string, int> wordCounts, int totalWords, int vocabSize)
        {
            var count = wordCounts.GetValueOrDefault(word, 0);
            return (count + _alpha) / (totalWords + _alpha * vocabSize);
        }
        
        private int GetVocabularySize()
        {
            var allWords = new HashSet<string>();
            allWords.UnionWith(_model.VeryPositiveWordCounts.Keys);
            allWords.UnionWith(_model.PositiveWordCounts.Keys);
            allWords.UnionWith(_model.NeutralWordCounts.Keys);
            allWords.UnionWith(_model.NegativeWordCounts.Keys);
            allWords.UnionWith(_model.VeryNegativeWordCounts.Keys);
            return allWords.Count;
        }
        
        private List<string> Tokenize(string text)
        {
            // Enhanced tokenization
            return text.ToLower()
                      .Split(' ', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}', '-', '_', '/', '\\')
                      .Select(word => word.Trim())
                      .Where(word => word.Length > 2 && !IsStopWord(word))
                      .Where(word => !string.IsNullOrWhiteSpace(word))
                      .Distinct() // Remove duplicates in same text
                      .ToList();
        }
        
        private bool IsStopWord(string word)
        {
            var stopWords = new HashSet<string> {
                "the", "and", "or", "but", "is", "are", "was", "were", "be", "been", "have", 
                "has", "had", "do", "does", "did", "will", "would", "could", "should", "can",
                "may", "might", "must", "this", "that", "these", "those", "them", "then", "than"
            };
            return stopWords.Contains(word);
        }
        
        public void SaveModel()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_model, options);
                File.WriteAllText(_modelPath, json);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[SUCCESS] Model saved to {_modelPath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Error saving model: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public void LoadModel()
        {
            try
            {
                if (!File.Exists(_modelPath))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[WARNING] No saved model found, will train from scratch");
                    Console.ResetColor();
                    return;
                }
                
                var json = File.ReadAllText(_modelPath);
                var modelData = JsonSerializer.Deserialize<NaiveBayesModelData>(json);
                
                if (modelData != null)
                {
                    _model = modelData;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[SUCCESS] Model loaded from {_modelPath}");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"[INFO] Loaded model stats: {_model.TotalDocs} training examples");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Error loading model: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public Dictionary<string, object> GetModelInfo()
        {
            return new Dictionary<string, object>
            {
                ["TotalTrainingExamples"] = _model.TotalDocs,
                ["VocabularySize"] = GetVocabularySize(),
                ["ClassDistribution"] = new Dictionary<string, int>
                {
                    ["Very Positive"] = _model.VeryPositiveDocs,
                    ["Positive"] = _model.PositiveDocs,
                    ["Neutral"] = _model.NeutralDocs,
                    ["Negative"] = _model.NegativeDocs,
                    ["Very Negative"] = _model.VeryNegativeDocs
                },
                ["WordCounts"] = new Dictionary<string, int>
                {
                    ["Very Positive"] = _model.VeryPositiveTotalWords,
                    ["Positive"] = _model.PositiveTotalWords,
                    ["Neutral"] = _model.NeutralTotalWords,
                    ["Negative"] = _model.NegativeTotalWords,
                    ["Very Negative"] = _model.VeryNegativeTotalWords
                }
            };
        }
        
        // Add this new method to show top words
        private void ShowTopWordsByClass(int topN)
        {
            var classes = new[]
            {
                ("Very Positive", _model.VeryPositiveWordCounts),
                ("Positive", _model.PositiveWordCounts),
                ("Neutral", _model.NeutralWordCounts),
                ("Negative", _model.NegativeWordCounts),
                ("Very Negative", _model.VeryNegativeWordCounts)
            };
            
            foreach (var (className, wordCounts) in classes)
            {
                var topWords = wordCounts.OrderByDescending(kv => kv.Value).Take(topN).ToList();
                if (topWords.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"   {className}: {string.Join(", ", topWords.Select(kv => $"{kv.Key}({kv.Value})"))}");
                    Console.ResetColor();
                }
            }
        }
    }
}