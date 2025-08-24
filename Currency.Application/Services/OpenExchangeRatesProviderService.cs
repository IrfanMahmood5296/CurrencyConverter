using Currency.Application.Interfaces;
using Currency.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Currency.Application.Services
{
    [Authorize]
    public class OpenExchangeRatesProviderService: ICurrencyProvider
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private string? _appId;
        public OpenExchangeRatesProviderService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://v6.exchangerate-api.com/v6/");
            _configuration = configuration;
            _appId = _configuration["OpenExchangeRates:AppId"];
        }

        public async Task<RateResponse> GetLatestRatesAsync(RatesRequest ratesRequest)
        {
            var endpoint = $"{_appId}/latest/{ratesRequest.BaseCurrency}";
            var queryParams = new List<string> { $"app_id={_appId}" };

            var response = await _httpClient.GetFromJsonAsync<RateResponse>(endpoint);

            if (response == null)
                throw new Exception("Failed to fetch currency rates.");

            return response;
        }

        public async Task<HistoricalRateResponse> GetHistoricalExchangeRates(HistoricalRequest ratesRequest)
        {
            string endpoint;

            if (ratesRequest.StartDate.HasValue && ratesRequest.EndDate.HasValue)
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

        public async Task<ConvertExchangeRatesRequest> ConvertCurrencyAsync(ConvertExchangeRatesRequest ratesRequest)
        {
            if (string.IsNullOrWhiteSpace(ratesRequest.From) || string.IsNullOrWhiteSpace(ratesRequest.To))
                throw new ArgumentException("Currency codes must be provided.");


            var endpoint = $"latest?base={ratesRequest.From}&symbols={ratesRequest.To}";
            var response = await _httpClient.GetFromJsonAsync<RateResponseDto>(endpoint);

            if (response == null)
                throw new Exception("Failed to fetch conversion rate.");

            return new ConvertExchangeRatesRequest
            {
                Date = response.Date,
                From = ratesRequest.From,
                To = ratesRequest.To,
                Amount = ratesRequest.Amount,
                ConvertedAmounts = response.Rates.ToDictionary(kvp => kvp.Key, kvp => Math.Round(ratesRequest.Amount * kvp.Value, 2))
            };
        }
    }
}
