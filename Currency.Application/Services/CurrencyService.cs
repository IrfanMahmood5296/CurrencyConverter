using Currency.Application.Interfaces;
using Currency.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.frankfurter.app/");
            }
        }

        public async Task<RateResponse> GetLatestRatesAsync(string? baseCurrency, string? symbols = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            string endpoint ;

            if (startDate.HasValue && endDate.HasValue && startDate != endDate)
            {
                // Date range
                endpoint = $"{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}";
            }
            else if (startDate.HasValue)
            {
                // Single date
                endpoint = $"{startDate:yyyy-MM-dd}";
            }
            else
            {
                endpoint = "latest";
            }

            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(baseCurrency))
                queryParams.Add($"base={baseCurrency}");

            if (!string.IsNullOrWhiteSpace(symbols))
                queryParams.Add($"symbols={symbols}");

            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetFromJsonAsync<dynamic>(endpoint);

            if (endpoint.Contains("..")) // Historical range
            {
                var historical = await _httpClient.GetFromJsonAsync<HistoricalRateResponse>(endpoint);
                if (historical == null)
                    throw new Exception("Failed to fetch historical currency rates.");

                return new RateResponse
                {
                    Date = startDate ?? DateTime.MinValue,
                    DateTo = endDate ?? DateTime.MinValue,
                    HistoricalRates = historical.Rates
                };
            }
            else // Single/latest date
            {
                var single = await _httpClient.GetFromJsonAsync<RateResponse>(endpoint);
                if (single == null)
                    throw new Exception("Failed to fetch currency rates.");

                return single;
            }
        }

        public async Task<decimal> ConvertCurrencyAsync(string from, string to, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Currency codes must be provided.");

            if (to == "TRY" || to == "PLN" || to == "THB" || to == "MXN" ||
                from == "TRY" || from == "PLN" || from == "THB" || from == "MXN")
            {
                throw new InvalidOperationException("Conversion involving TRY, PLN, THB, or MXN is not allowed.");
            }

            var endpoint = $"latest?base={from}&symbols={to}";
            var response = await _httpClient.GetFromJsonAsync<RateResponseDto>(endpoint);

            if (response == null || !response.Rates.ContainsKey(to))
                throw new Exception("Failed to fetch conversion rate.");

            return Math.Round(amount * response.Rates[to], 2);
        }
    }
}