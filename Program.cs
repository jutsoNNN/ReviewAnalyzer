using ReviewAnalyzer.BusinessLogic.Factories;
using ReviewAnalyzer.Infrastructure.AI;
using ReviewAnalyzer.Infrastructure.Storage;
using ReviewAnalyzer.Presentation;
using ReviewAnalyzer.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ReviewAnalyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var config = AppSettings.Load();

                Logger.Info($"Application started at {DateTime.Now}");
                Logger.Info($"Base directory: {AppContext.BaseDirectory}");
                Logger.Info($"Using model: {config.ModelPath}");
                Logger.Info($"GPU layers: {config.GpuLayers}");
                Logger.Info($"Max reviews: {config.MaxReviews}");
                Logger.Info($"Context size: {config.ContextSize}");
                Logger.Info($"Timeout: {config.AnalysisTimeout}s");

                if (!File.Exists(config.ModelPath))
                {
                    Logger.Error($"Model file not found: {config.ModelPath}");
                    Logger.Info($"Current directory: {Environment.CurrentDirectory}");

                    var alternativePath = Path.Combine(AppContext.BaseDirectory, "Models", "deepseek-llm-7b-chat.Q4_K_M.gguf");
                    if (File.Exists(alternativePath))
                    {
                        Logger.Info($"Found model at alternative location: {alternativePath}");
                        config.ModelPath = alternativePath;
                    }
                    else
                    {
                        Logger.Info("Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                }

                var storage = new JsonStorageService();
                var analyzer = new DeepSeekAnalysisService(
                    config.ModelPath,
                    config.ContextSize,
                    config.GpuLayers,
                    config.AnalysisTimeout);

                var adapterFactory = new MarketplaceAdapterFactory();
                var consoleInterface = new ConsoleInterface(
                    adapterFactory,
                    analyzer,
                    storage,
                    config.MaxReviews);

                await consoleInterface.Run();
            }
            catch (Exception ex)
            {
                Logger.Error("Critical application error", ex);
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}