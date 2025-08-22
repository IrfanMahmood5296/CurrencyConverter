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
    public class RateService : ICurrencyService
    {
        private readonly HttpClient _httpClient;

        public RateService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress = new Uri("https://api.frankfurter.app/");
        }

        public async Task<RateResponse> GetLatestRatesAsync(RatesRequest ratesRequest)
        {
            string endpoint ;

            if (ratesRequest.StartDate.HasValue)
            {
                endpoint = $"{ratesRequest.StartDate:yyyy-MM-dd}";
            }
            else
            {
                endpoint = "latest";
            }

            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(ratesRequest.BaseCurrency))
                queryParams.Add($"base={ratesRequest.BaseCurrency}");

            if (!string.IsNullOrWhiteSpace(ratesRequest.Symbols))
                queryParams.Add($"symbols={ratesRequest.Symbols}");

            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetFromJsonAsync<RateResponse>(endpoint);
            if (response == null)
                throw new Exception("Failed to fetch currency rates.");

            return response;
        }

        public async Task<HistoricalRateResponse> GetHistoricalExchangeRates(HistoricalRequest ratesRequest)
        {
            string endpoint;

            if (ratesRequest.StartDate.HasValue && ratesRequest.EndDate.HasValue )
            {
                endpoint = $"{ratesRequest.StartDate:yyyy-MM-dd}..{ratesRequest.EndDate:yyyy-MM-dd}";
            }
            else
            {
                endpoint = "latest";
            }

            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(ratesRequest.BaseCurrency))
                queryParams.Add($"base={ratesRequest.BaseCurrency}");

            if (!string.IsNullOrWhiteSpace(ratesRequest.Symbols))
                queryParams.Add($"symbols={ratesRequest.Symbols}");

            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);

            var historical = await _httpClient.GetFromJsonAsync<HistoricalRateResponse>(endpoint);
            if (historical == null)
                throw new Exception("Failed to fetch historical currency rates.");

            return new HistoricalRateResponse
            {
                Date = ratesRequest.StartDate ?? DateTime.MinValue,
                DateTo = ratesRequest.EndDate ?? DateTime.MinValue,
                HistoricalRates = historical.HistoricalRates
            };
        }

        public async Task<decimal> ConvertCurrencyAsync(ConvertExchangeRatesRequest ratesRequest)
        {
            if (string.IsNullOrWhiteSpace(ratesRequest.From) || string.IsNullOrWhiteSpace(ratesRequest.To))
                throw new ArgumentException("Currency codes must be provided.");


            var endpoint = $"latest?base={ratesRequest.From}&symbols={ratesRequest.To}";
            var response = await _httpClient.GetFromJsonAsync<RateResponseDto>(endpoint);

            if (response == null || !response.Rates.ContainsKey(ratesRequest.To))
                throw new Exception("Failed to fetch conversion rate.");

            return Math.Round(ratesRequest.Amount * response.Rates[ratesRequest.To], 2);
        }
    }
}