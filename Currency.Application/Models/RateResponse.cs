using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Models
{
    public class RateResponse
    {
        public DateTime? Date { get; set; } 
        public DateTime? DateTo { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
        public Dictionary<string, Dictionary<string, decimal>>? HistoricalRates { get; set; }
    }

    public class HistoricalRateResponse
    {
        public Dictionary<string, Dictionary<string, decimal>>? Rates { get; set; }
    }
}