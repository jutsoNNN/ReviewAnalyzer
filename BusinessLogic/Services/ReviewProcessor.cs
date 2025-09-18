using ReviewAnalyzer.Core.Entities;
using ReviewAnalyzer.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewAnalyzer.BusinessLogic.Services
{
    public class ReviewProcessor
    {
        private readonly IMarketplaceAdapter _marketplaceAdapter;
        private readonly IReviewAnalyzer _analyzer;
        private readonly IDataStorage _storage;

        public ReviewProcessor(
            IMarketplaceAdapter marketplaceAdapter,
            IReviewAnalyzer analyzer,
            IDataStorage storage)
        {
            _marketplaceAdapter = marketplaceAdapter;
            _analyzer = analyzer;
            _storage = storage;
        }

        public async Task<AnalysisResult> AnalyzeProductAsync(string productUrl, int maxReviews)
        {
            var productId = _marketplaceAdapter.GetProductId(productUrl);
            var reviews = await _marketplaceAdapter.FetchReviewsAsync(productUrl, maxReviews);

            if (reviews.Count == 0)
            {
                var emptyResult = new AnalysisResult
                {
                    ProductId = productId,
                    OverallSentiment = "No reviews found",
                    PurchaseRecommendation = "No data available"
                };

                var emptyAnalysisFileName = $"analysis_{productId}.json";
                _storage.Save(emptyAnalysisFileName, emptyResult);

                return emptyResult;
            }

            var reviewsFileName = $"reviews_{productId}.json";
            _storage.Save(reviewsFileName, reviews);

            var result = await _analyzer.AnalyzeAsync(reviews);
            result.ProductId = productId;

            var resultAnalysisFileName = $"analysis_{result.ProductId}.json";
            _storage.Save(resultAnalysisFileName, result);

            return result;
        }
    }
}