using ReviewAnalyzer.BusinessLogic.Factories;
using ReviewAnalyzer.BusinessLogic.Services;
using ReviewAnalyzer.Core.Enums;
using ReviewAnalyzer.Core.Interfaces;
using System;
using System.Threading.Tasks;
using ReviewAnalyzer.Utilities;

namespace ReviewAnalyzer.Presentation
{
    public class ConsoleInterface
    {
        private readonly MarketplaceAdapterFactory _adapterFactory;
        private readonly IReviewAnalyzer _analyzer;
        private readonly IDataStorage _storage;
        private readonly int _maxReviews;

        public ConsoleInterface(
            MarketplaceAdapterFactory adapterFactory,
            IReviewAnalyzer analyzer,
            IDataStorage storage,
            int maxReviews)
        {
            _adapterFactory = adapterFactory;
            _analyzer = analyzer;
            _storage = storage;
            _maxReviews = maxReviews;
        }

        public async Task Run()
        {
            Console.WriteLine("=== PRODUCT REVIEW ANALYZER (DeepSeek R1) ===");

            Console.Write("Enter product URL: ");
            var url = Console.ReadLine() ?? "";
            var config = AppSettings.Load();

            Console.WriteLine("\nSelect marketplace:");
            Console.WriteLine("1 - Ozon(пока не работает)");
            Console.WriteLine("2 - Wildberries");
            Console.Write("Your choice: ");
            var choice = int.TryParse(Console.ReadLine(), out int c) ? c : 1;
            var marketplace = (MarketplaceType)(choice - 1);

            var adapter = _adapterFactory.CreateAdapter(marketplace);

            Console.WriteLine("\nStarting analysis...");
            var processor = new ReviewProcessor(adapter, _analyzer, _storage);
            Console.WriteLine($"Сырые отзывы сохранены в папку: ParsedReviews");
            var result = await processor.AnalyzeProductAsync(url, _maxReviews);

            if (result.OverallSentiment == "No reviews found")
            {
                Console.WriteLine("\nANALYSIS COMPLETED!\n");
                Console.WriteLine("No reviews found for this product.");
            }
            else
            {
                Console.WriteLine("\nАНАЛИЗ ЗАВЕРШЕН!\n");
                Console.WriteLine(ReportGenerator.GenerateConsoleReport(result));
            }
            Console.WriteLine($"\nРезультаты анализа сохранены в папку: AnalysisResults");
            Console.WriteLine($"\nРезультаты сохранены в analysis_{result.ProductId}.json");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}