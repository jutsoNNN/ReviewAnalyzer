using System;
using System.IO;
using Newtonsoft.Json;

namespace ReviewAnalyzer.Utilities
{
    public class AppSettings
    {
        public string ModelPath { get; set; } = "";
        public int ContextSize { get; set; } = 4096;
        public int GpuLayers { get; set; } = 0;
        public int MaxReviews { get; set; } = 20;
        public int AnalysisTimeout { get; set; } = 300;

        public static AppSettings Load()
        {
            var config = new AppSettings();
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    config = JsonConvert.DeserializeObject<AppSettings>(json) ?? config;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error loading appsettings.json", ex);
                }
            }
            else
            {
                Logger.Error("appsettings.json not found!");
            }

            if (!string.IsNullOrEmpty(config.ModelPath))
            {
                if (!Path.IsPathRooted(config.ModelPath))
                {
                    config.ModelPath = Path.Combine(AppContext.BaseDirectory, config.ModelPath);
                }
            }
            else
            {
                config.ModelPath = Path.Combine(AppContext.BaseDirectory, "Models", "deepseek-llm-7b-chat.Q4_K_M.gguf");
                Logger.Warning($"Using default model path: {config.ModelPath}");
            }

            Logger.Info($"Загружены настройки: ContextSize={config.ContextSize}, MaxReviews={config.MaxReviews}");

            return config;
        }
    }
}