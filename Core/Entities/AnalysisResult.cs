using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewAnalyzer.Core.Entities
{
    public class AnalysisResult
    {
        public string ProductId { get; set; } = "";
        public string OverallSentiment { get; set; } = "";
        public List<string> KeyPros { get; } = new List<string>();
        public List<string> KeyCons { get; } = new List<string>();
        public List<string> CommonThemes { get; } = new List<string>();
        public string PurchaseRecommendation { get; set; } = "";
    }
}