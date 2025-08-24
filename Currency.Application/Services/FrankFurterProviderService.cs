using Currency.Application.Interfaces;
using Currency.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Currency.Application.Services
{
    [Authorize]
    public class FrankFurterProviderService : ICurrencyProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FrankFurterProviderService> _logger;

        public FrankFurterProviderService(HttpClient httpClient, ILogger<FrankFurterProviderService> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.frankfurter.app/");
        }

        public async Task<RateResponse> GetLatestRatesAsync(RatesRequest ratesRequest)
        {
            _logger.LogInformation("Fetching latest rates. BaseCurrency={Base}, Symbols={Symbols}, StartDate={StartDate}",
                ratesRequest.BaseCurrency, ratesRequest.Symbols, ratesRequest.StartDate);

            try
            {
                string endpoint;

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

                _logger.LogDebug("Requesting endpoint: {Endpoint}", endpoint);

                var response = await _httpClient.GetFromJsonAsync<RateResponse>(endpoint);

                if (response == null)
                {
                    _logger.LogError("Failed to fetch currency rates from endpoint {Endpoint}", endpoint);
                    throw new Exception("Failed to fetch currency rates.");
                }

                _logger.LogInformation("Successfully fetched latest rates for Base={Base}", ratesRequest.BaseCurrency);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching latest rates");
                throw;
            }
        }

        public async Task<HistoricalRateResponse> GetHistoricalExchangeRates(HistoricalRequest ratesRequest)
        {
            _logger.LogInformation("Fetching historical rates. Base={Base}, Symbols={Symbols}, From={From}, To={To}",
                ratesRequest.BaseCurrency, ratesRequest.Symbols, ratesRequest.StartDate, ratesRequest.EndDate);

            try
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

                _logger.LogDebug("Requesting endpoint: {Endpoint}", endpoint);

                var historical = await _httpClient.GetFromJsonAsync<HistoricalRateResponse>(endpoint);
                if (historical == null)
                {
                    _logger.LogError("Failed to fetch historical rates from endpoint {Endpoint}", endpoint);
                    throw new Exception("Failed to fetch historical currency rates.");
                }

                _logger.LogInformation("Successfully fetched historical rates from {From} to {To}",
                    ratesRequest.StartDate, ratesRequest.EndDate);

                return new HistoricalRateResponse
                {
                    Date = ratesRequest.StartDate ?? DateTime.MinValue,
                    DateTo = ratesRequest.EndDate ?? DateTime.MinValue,
                    HistoricalRates = historical.HistoricalRates
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching historical rates");
                throw;
            }
        }

        public async Task<ConvertExchangeRatesRequest> ConvertCurrencyAsync(ConvertExchangeRatesRequest ratesRequest)
        {
            _logger.LogInformation("Converting currency. From={From}, To={To}, Amount={Amount}",
                ratesRequest.From, ratesRequest.To, ratesRequest.Amount);

            try
            {
                if (string.IsNullOrWhiteSpace(ratesRequest.From) || string.IsNullOrWhiteSpace(ratesRequest.To))
                {
                    _logger.LogWarning("Invalid conversion request: From={From}, To={To}", ratesRequest.From, ratesRequest.To);
                    throw new ArgumentException("Currency codes must be provided.");
                }

                var endpoint = $"latest?base={ratesRequest.From}&symbols={ratesRequest.To}";
                _logger.LogDebug("Requesting conversion endpoint: {Endpoint}", endpoint);

                var response = await _httpClient.GetFromJsonAsync<RateResponseDto>(endpoint);

                if (response == null)
                {
                    _logger.LogError("Failed to fetch conversion rate from endpoint {Endpoint}", endpoint);
                    throw new Exception("Failed to fetch conversion rate.");
                }

                var result = new ConvertExchangeRatesRequest
                {
                    Date = response.Date,
                    From = ratesRequest.From,
                    To = ratesRequest.To,
                    Amount = ratesRequest.Amount,
                    ConvertedAmounts = response.Rates.ToDictionary(
                        kvp => kvp.Key,
                        kvp => Math.Round(ratesRequest.Amount * kvp.Value, 2))
                };

                _logger.LogInformation("Conversion completed. From={From}, To={To}, Amount={Amount}, Converted={Converted}",
                    ratesRequest.From, ratesRequest.To, ratesRequest.Amount,
                    string.Join(", ", result.ConvertedAmounts.Select(kvp => $"{kvp.Key}:{kvp.Value}")));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while converting currency. From={From}, To={To}, Amount={Amount}",
                    ratesRequest.From, ratesRequest.To, ratesRequest.Amount);
                throw;
            }
        }
    }
}