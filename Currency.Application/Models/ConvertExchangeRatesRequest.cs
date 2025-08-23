using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Models
{
    public class ConvertExchangeRatesRequest
    {
        public required string From { get; set; }
        public required string To { get; set; }
        public required decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> ConvertedAmounts { get; set; } = new();
    }
}
