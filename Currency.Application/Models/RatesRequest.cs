using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Models
{
    public class RatesRequest
    {
        public string? BaseCurrency { get; set; } 
        public string? Symbols { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
    }

    public class HistoricalRequest
    {
        public string? BaseCurrency { get; set; } 
        public string? Symbols { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
    }
}
