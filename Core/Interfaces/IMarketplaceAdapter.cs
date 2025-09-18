using ReviewAnalyzer.Core.Entities;
using ReviewAnalyzer.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewAnalyzer.Core.Interfaces
{
    public interface IMarketplaceAdapter
    {
        Task<List<ProductReview>> FetchReviewsAsync(string productUrl, int maxReviews);
        MarketplaceType MarketplaceType { get; }
        string GetProductId(string productUrl);
    }
}