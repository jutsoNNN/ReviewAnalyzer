using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewAnalyzer.Core.Entities
{
    public class ProductReview
    {
        public string Content { get; set; } = "";
        public int Rating { get; set; }
        public string Source { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.MinValue;
        public string ProductId { get; set; } = "";
    }
}