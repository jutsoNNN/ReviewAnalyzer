using ReviewAnalyzer.Core.Entities;
using System.Text;

namespace ReviewAnalyzer.Presentation
{
    public static class ReportGenerator
    {
        public static string GenerateConsoleReport(AnalysisResult result)
        {
            var report = new StringBuilder();
            report.AppendLine("========== ОТЧЕТ АНАЛИЗА ПРОДУКТА ==========");
            report.AppendLine($"ID продукта: {result.ProductId}");
            report.AppendLine($"Общая тональность: {result.OverallSentiment}");
            report.AppendLine($"Рекомендация: {result.PurchaseRecommendation}");
            report.AppendLine("");

            report.AppendLine("Ключевые преимущества:");
            foreach (var pro in result.KeyPros)
                report.AppendLine($"- {pro}");

            report.AppendLine("");
            report.AppendLine("Ключевые недостатки:");
            foreach (var con in result.KeyCons)
                report.AppendLine($"- {con}");

            report.AppendLine("");
            report.AppendLine("Основные темы:");
            foreach (var theme in result.CommonThemes)
                report.AppendLine($"- {theme}");

            report.AppendLine("=============================================");
            return report.ToString();
        }
    }
}