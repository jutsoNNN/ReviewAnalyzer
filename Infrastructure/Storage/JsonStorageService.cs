using Newtonsoft.Json;
using ReviewAnalyzer.Core.Entities;
using ReviewAnalyzer.Core.Interfaces;
using System.IO;

namespace ReviewAnalyzer.Infrastructure.Storage
{
    public class JsonStorageService : IDataStorage
    {
        public string Save<T>(string fileName, T data)
        {
            string subfolder = typeof(T) == typeof(List<ProductReview>)
                ? "ParsedReviews"
                : "AnalysisResults";

            Directory.CreateDirectory(subfolder);
            string fullPath = Path.Combine(subfolder, fileName);

            if (File.Exists(fullPath)) File.Delete(fullPath);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(fileName, json);
            return Path.GetFullPath(fileName);
        }
        public T? Load<T>(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}