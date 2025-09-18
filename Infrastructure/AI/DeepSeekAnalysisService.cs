using LLama;
using LLama.Common;
using ReviewAnalyzer.Core.Entities;
using ReviewAnalyzer.Core.Interfaces;
using ReviewAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReviewAnalyzer.Infrastructure.AI
{
    public class DeepSeekAnalysisService : IReviewAnalyzer
    {
        private readonly LLamaWeights _model;
        private readonly ModelParams _params;
        private readonly int _timeoutSeconds;
        private readonly string _modelPath;

        public DeepSeekAnalysisService(string modelPath, int contextSize, int gpuLayers, int timeoutSeconds)
        {
            _modelPath = modelPath;

            if (!File.Exists(modelPath))
            {
                Logger.Error($"Model file not found: {modelPath}");
                throw new FileNotFoundException($"Model file not found: {modelPath}");
            }

            _params = new ModelParams(modelPath)
            {
                ContextSize = (uint)contextSize,
                GpuLayerCount = gpuLayers,
                Seed = 1337,
                UseMemorymap = true,
                UseMemoryLock = false
            };

            Logger.Info($"Loading model: {Path.GetFileName(modelPath)}");
            Logger.Info($"Context size: {contextSize}, GPU layers: {gpuLayers}");

            try
            {
                LoadNativeLibrary();
                _model = LLamaWeights.LoadFromFile(_params);
                Logger.Info("Model loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load model", ex);
                throw;
            }

            _timeoutSeconds = timeoutSeconds;
        }

        private void LoadNativeLibrary()
        {
            try
            {
                string libraryName = "llama.dll";
                string basePath = AppContext.BaseDirectory;

                string[] pathsToTry = {
                    Path.Combine(basePath, "runtimes", "win-x64", "native", libraryName),
                    Path.Combine(basePath, "runtimes", "win-x64", "native", "avx2", libraryName),
                    Path.Combine(basePath, "runtimes", "win-x64", "native", "avx512", libraryName),
                    Path.Combine(basePath, libraryName)
                };

                foreach (var path in pathsToTry)
                {
                    if (File.Exists(path))
                    {
                        Logger.Info($"Loading native library from: {path}");
                        NativeLibrary.Load(path);
                        return;
                    }
                }

                throw new FileNotFoundException($"Native library '{libraryName}' not found in any expected locations");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load native library", ex);
                throw;
            }
        }

        public async Task<AnalysisResult> AnalyzeAsync(List<ProductReview> reviews)
        {
            try
            {
                if (reviews == null || reviews.Count == 0)
                {
                    Logger.Warning("No reviews to analyze");
                    return new AnalysisResult
                    {
                        OverallSentiment = "No reviews",
                        PurchaseRecommendation = "No data"
                    };
                }

                var prompt = BuildAnalysisPrompt(reviews);
                Logger.Info($"Starting analysis with {reviews.Count} reviews...");
                var result = await ProcessWithModel(prompt);
                Logger.Info("Analysis completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Analysis failed", ex);
                return new AnalysisResult
                {
                    OverallSentiment = "Error",
                    PurchaseRecommendation = "Analysis failed"
                };
            }
        }

        private async Task<AnalysisResult> ProcessWithModel(string prompt)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
            using var context = _model.CreateContext(_params);
            var executor = new InteractiveExecutor(context);

            var inferenceParams = new InferenceParams
            {
                Temperature = 0.1f,
                AntiPrompts = new List<string> { "}" },
                MaxTokens = 4096,
                FrequencyPenalty = 0.5f
            };

            var response = new StringBuilder();
            Logger.Info("Starting model inference...");

            try
            {
                await foreach (var text in executor.InferAsync(prompt, inferenceParams, cts.Token))
                {
                    response.Append(text);
                    if (text.Contains("}")) break;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Error("Analysis timed out");
            }

            Logger.Info("Model inference completed");
            return ExtractResultFromResponse(response.ToString());
        }

        private AnalysisResult ExtractResultFromResponse(string response)
        {
            try
            {
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}') + 1;

                if (jsonStart < 0 || jsonEnd <= jsonStart)
                {
                    Logger.Error("Invalid response format - no JSON found");
                    Logger.Error($"Response snippet: {TruncateResponse(response, 200)}");
                    return new AnalysisResult
                    {
                        OverallSentiment = "Parser error",
                        PurchaseRecommendation = "Invalid AI response"
                    };
                }

                var json = response[jsonStart..jsonEnd];
                Logger.Info($"Extracted JSON: {json}");

                return Newtonsoft.Json.JsonConvert.DeserializeObject<AnalysisResult>(json)
                    ?? new AnalysisResult();
            }
            catch (Exception ex)
            {
                Logger.Error("Error parsing model response", ex);
                return new AnalysisResult
                {
                    OverallSentiment = "Parser error",
                    PurchaseRecommendation = "JSON parse failed"
                };
            }
        }

        private string TruncateResponse(string response, int maxLength)
        {
            return response.Length <= maxLength
                ? response
                : response[..maxLength] + "...";
        }

        private string BuildAnalysisPrompt(List<ProductReview> reviews)
        {
            var sb = new StringBuilder();
            sb.AppendLine("### System Prompt");

            if (reviews.Count > 0)
            {
                var firstReview = reviews[0];
                sb.AppendLine($"\nПродукт: {firstReview.Source}, ID: {firstReview.ProductId}");
            }

            sb.AppendLine("### Строгий формат JSON-анализа");
            sb.AppendLine("Ты ДОЛЖЕН предоставить анализ отзывов ТОЛЬКО в этом формате JSON БЕЗ дополнительного текста. Правила:");
            sb.AppendLine("1. OverallSentiment: Одно из [Крайне положительные, Положительные, Смешанные, Отрицательные, Крайне отрицательные]");
            sb.AppendLine("2. KeyPros/KeyCons/CommonThemes: Массивы из 3-15 кратких фраз на русском");
            sb.AppendLine("3. PurchaseRecommendation: Одно из [Да, Нет, С оговорками]");
            sb.AppendLine("4. НИКОГДА не добавляй пояснения, заметки или текст вне JSON-структуры");

            sb.AppendLine(@"{
  ""OverallSentiment"": """",
  ""KeyPros"": [""""],
  ""KeyCons"": [""""],
  ""CommonThemes"": [""""],
  ""PurchaseRecommendation"": """"
}");
            sb.AppendLine("");
            sb.AppendLine("### Отзывы");

            int reviewCount = Math.Min(reviews.Count, 20);
            for (int i = 0; i < reviewCount; i++)
            {
                var review = reviews[i];
                sb.AppendLine($"[Отзыв {i + 1}/{reviewCount}, Рейтинг: {review.Rating}/5] {review.Content}");
            }

            sb.AppendLine("### Результат анализа (ТОЛЬКО JSON):");
            return sb.ToString();
        }
    }
}