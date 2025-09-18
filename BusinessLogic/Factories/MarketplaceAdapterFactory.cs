using ReviewAnalyzer.Core.Enums;
using ReviewAnalyzer.Core.Interfaces;
using ReviewAnalyzer.Infrastructure.MarketplaceAdapters;
using System;

namespace ReviewAnalyzer.BusinessLogic.Factories
{
    public class MarketplaceAdapterFactory
    {
        public IMarketplaceAdapter CreateAdapter(MarketplaceType type)
        {
            return type switch
            {
                MarketplaceType.Ozon => new OzonAdapter(),
                MarketplaceType.Wildberries => new WildberriesAdapter(),
                _ => throw new ArgumentException("Unsupported marketplace type")
            };
        }
    }
}