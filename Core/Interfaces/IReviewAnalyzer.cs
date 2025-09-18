using ReviewAnalyzer.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewAnalyzer.Core.Interfaces
{
    public interface IReviewAnalyzer
    {
        Task<AnalysisResult> AnalyzeAsync(List<ProductReview> reviews);
    }
}