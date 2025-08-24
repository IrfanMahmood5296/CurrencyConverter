using Currency.Application.Interfaces;
using Currency.Application.Interfaces.Redis;
using Currency.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Currency.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly IProviderFactoryService _providerFactoryService;
        private readonly IRedisCacheService _cacheService;

        public RatesController(IProviderFactoryService providerFactoryService, IRedisCacheService cacheService)
        {
            _providerFactoryService = providerFactoryService;
            _cacheService = cacheService;
        }

        [HttpPost("GetLatestRatesAsync")]
        public async Task<IActionResult> GetLatestRatesAsync([FromBody] RatesRequest ratesRequest)
        {
            try
            {
                var providerName = User.Claims.FirstOrDefault(c => c.Type == "currency_provider")?.Value;

                if (string.IsNullOrEmpty(providerName))
                    return BadRequest("Currency provider not assigned to user.");

                var cacheKey = $"{providerName}:latest:{ratesRequest.BaseCurrency}_{ratesRequest.Symbols ?? ""}_{ratesRequest.StartDate ?? null} ";

                var cachedResult = await _cacheService.GetAsync<object>(cacheKey);
                if (cachedResult != null)
                {
                    return Ok(new { data = cachedResult });
                }

                var service = _providerFactoryService.GetRequiredService(providerName);

                var result = await service.GetLatestRatesAsync(ratesRequest);

                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1));

                return Ok(result);
            }
            catch (Exception ex)
            {
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
                    return BadRequest("Currency provider not assigned to user.");

                var cacheKey = $"{providerName}:historical:{ratesRequest.StartDate:yyyy-MM-dd}_{ratesRequest.EndDate:yyyy-MM-dd}:{ratesRequest.BaseCurrency}_{ratesRequest.Symbols ?? ""}";

                var cachedResult = await _cacheService.GetAsync<object>(cacheKey);
                if (cachedResult != null)
                {
                    return Ok(new { data = cachedResult });
                }

                var service = _providerFactoryService.GetRequiredService(providerName);

                var result = await service.GetHistoricalExchangeRates(ratesRequest);
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1));

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }

        [HttpPost("ConvertExchangeRates")]
        public async Task<IActionResult> Convert([FromBody] ConvertExchangeRatesRequest ratesRequest)
        {
            try
            {
                var providerName = User.Claims.FirstOrDefault(c => c.Type == "currency_provider")?.Value;

                if (string.IsNullOrEmpty(providerName))
                    return BadRequest("Currency provider not assigned to user.");

                var service = _providerFactoryService.GetRequiredService(providerName);

                var result = await service.ConvertCurrencyAsync(ratesRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
