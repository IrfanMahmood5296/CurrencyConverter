using Currency.Application.Constants;
using Currency.Application.Interfaces;
using Currency.Application.Interfaces.Redis;
using Currency.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Currency.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly IProviderFactoryService _providerFactoryService;
        private readonly IRedisCacheService _cacheService;
        private readonly ILogger<RatesController> _logger;

        public RatesController(IProviderFactoryService providerFactoryService, IRedisCacheService cacheService, ILogger<RatesController> logger)
        {
            _providerFactoryService = providerFactoryService;
            _cacheService = cacheService;
            _logger = logger;

        }

        [HttpPost("GetLatestRatesAsync")]
        public async Task<IActionResult> GetLatestRatesAsync([FromBody] RatesRequest ratesRequest)
        {
            try
            {
                var providerName = User.Claims.FirstOrDefault(c => c.Type == "currency_provider")?.Value;

                if (string.IsNullOrEmpty(providerName))
                    return BadRequest(new
                    {
                        Error = "ProviderNotAssigned",
                        Message = "Currency provider not assigned to user.",
                        Timestamp = DateTime.UtcNow
                    });

                var cacheKey = $"{providerName}:latest:{ratesRequest.BaseCurrency ?? ""}_{ratesRequest.Symbols ?? ""}_{ratesRequest.StartDate ?? null}";

                var cachedResult = await _cacheService.GetAsync<object>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                    return Ok(cachedResult);
                }

                var service = _providerFactoryService.GetRequiredService(providerName);

                var result = await service.GetLatestRatesAsync(ratesRequest);

                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                return BadRequest(new { error = ex.Message });
            }

        }

        [HttpPost("GetHistoricalExchangeRates")]
        public async Task<IActionResult> GetHistoricalExchangeRates([FromBody] HistoricalRequest ratesRequest)
        {
            try
            {
                var providerName = User.Claims.FirstOrDefault(c => c.Type == "currency_provider")?.Value;

                if (string.IsNullOrEmpty(providerName))
                    return BadRequest(new
                    {
                        Error = "ProviderNotAssigned",
                        Message = "Currency provider not assigned to user.",
                        Timestamp = DateTime.UtcNow
                    });

                var cacheKey = $"{providerName}:historical:{ratesRequest.StartDate:yyyy-MM-dd}_{ratesRequest.EndDate:yyyy-MM-dd}:{ratesRequest.BaseCurrency ?? ""}_{ratesRequest.Symbols ?? ""}";

                var cachedResult = await _cacheService.GetAsync<object>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                    return Ok(cachedResult);
                }

                var service = _providerFactoryService.GetRequiredService(providerName);

                var result = await service.GetHistoricalExchangeRates(ratesRequest);
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("ConvertExchangeRates")]
        public async Task<IActionResult> Convert([FromBody] ConvertExchangeRatesRequest ratesRequest)
        {
            try
            {
                if (!ratesRequest.IsValid(out var validationErrorMessage))
                {
                    _logger.LogWarning("Currency conversion attempted with excluded currency: {From} to {To}",
                        ratesRequest.From, ratesRequest.To);

                    return BadRequest(new
                    {
                        Error = "UnsupportedCurrency",
                        Message = validationErrorMessage,
                    });
                }

                var providerName = User.Claims.FirstOrDefault(c => c.Type == "currency_provider")?.Value;

                if (string.IsNullOrEmpty(providerName))
                    return BadRequest(new
                    {
                        Error = "ProviderNotAssigned",
                        Message = "Currency provider not assigned to user.",
                        Timestamp = DateTime.UtcNow
                    });

                var service = _providerFactoryService.GetRequiredService(providerName);

                var result = await service.ConvertCurrencyAsync(ratesRequest);

                _logger.LogInformation("Currency conversion successful: {Amount} {From} to {To}",
                    ratesRequest.Amount, ratesRequest.From, ratesRequest.To);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Currency conversion failed: {From} to {To}",
                    ratesRequest.From, ratesRequest.To);

                return BadRequest(new
                {
                    Error = "ConversionFailed",
                    Message = "An error occurred during currency conversion",
                    Details = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
