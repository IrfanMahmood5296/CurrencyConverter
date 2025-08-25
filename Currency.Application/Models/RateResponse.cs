using System.Text.Json.Serialization;

namespace Currency.Application.Models
{
    public class RateResponse
    {
        [JsonPropertyName("base")]
        public string BaseCurrency { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }

    public class HistoricalRateResponse
    {
        [JsonPropertyName("base")]
        public string BaseCurrency { get; set; } = string.Empty;

        [JsonPropertyName("start_date")]
        public DateTime? Date { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? DateTo { get; set; }

        [JsonPropertyName("rates")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, Dictionary<string, decimal>>? HistoricalRates { get; set; }
        public List<HistoricalRateItem> items { get; set; } = new();
        // Pagination
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
    public class HistoricalRateItem
    {
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = [];
    }
    public class ExchangeRateResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = string.Empty;

        [JsonPropertyName("documentation")]
        public string Documentation { get; set; } = string.Empty;

        [JsonPropertyName("terms_of_use")]
        public string TermsOfUse { get; set; } = string.Empty;

        [JsonPropertyName("time_last_update_unix")]
        public long TimeLastUpdateUnix { get; set; }

        [JsonPropertyName("time_last_update_utc")]
        public string TimeLastUpdateUtc { get; set; } = string.Empty;

        [JsonPropertyName("time_next_update_unix")]
        public long TimeNextUpdateUnix { get; set; }

        [JsonPropertyName("time_next_update_utc")]
        public string TimeNextUpdateUtc { get; set; } = string.Empty;

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = string.Empty;

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; } = [];
    }

}