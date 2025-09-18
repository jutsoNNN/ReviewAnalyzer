using AngleSharp;
using AngleSharp.Dom;
using ReviewAnalyzer.Core.Entities;
using ReviewAnalyzer.Core.Enums;
using ReviewAnalyzer.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// Не работает, необходима доработка по аналогии с Wildberries. YandexMarket был временно вырезан.

namespace ReviewAnalyzer.Infrastructure.MarketplaceAdapters
{
    public class OzonAdapter : IMarketplaceAdapter
    {
        public MarketplaceType MarketplaceType => MarketplaceType.Ozon;
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<ProductReview>> FetchReviewsAsync(string productUrl, int maxReviews)
        {
            var reviews = new List<ProductReview>();
            int page = 1;

            while (reviews.Count < maxReviews)
            {
                var pageUrl = $"{productUrl}?page={page}";
                var html = await _httpClient.GetStringAsync(pageUrl);

                var context = BrowsingContext.New(AngleSharp.Configuration.Default);

                var document = await context.OpenAsync(req => req.Content(html));

                var reviewNodes = document.QuerySelectorAll(".review-item");
                if (!reviewNodes.Any()) break;

                foreach (var node in reviewNodes)
                {
                    if (reviews.Count >= maxReviews) break;

                    var content = node.QuerySelector(".review-content")?.TextContent?.Trim() ?? "";
                    var ratingNode = node.QuerySelector(".rating");
                    var rating = ratingNode != null ? int.Parse(ratingNode.GetAttribute("data-value")) : 0;

                    reviews.Add(new ProductReview
                    {
                        Content = content,
                        Rating = rating,
                        Source = "Ozon",
                        ProductId = GetProductId(productUrl)
                    });
                }

                page++;
                await Task.Delay(1000);
            }

            return reviews;
        }

        public string GetProductId(string productUrl)
        {
            var match = Regex.Match(productUrl, @"product/(\d+)");
            return match.Success ? match.Groups[1].Value : "unknown";
        }
    }
}