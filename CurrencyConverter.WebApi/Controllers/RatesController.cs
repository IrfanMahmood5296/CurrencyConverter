using Currency.Application.Interfaces;
using Currency.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Currency.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly IProviderFactoryService _providerFactoryService;
        public RatesController(IProviderFactoryService providerFactoryService)
        {
            _providerFactoryService = providerFactoryService;
        }

        [HttpPost("GetLatestRatesAsync")]
        public async Task<IActionResult> GetLatestRatesAsync([FromBody] RatesRequest ratesRequest)
        {
            var providerName = User.Claims.FirstOrDefault(c => c.Type == "currency_provider")?.Value;

            if (string.IsNullOrEmpty(providerName))
                return BadRequest("Currency provider not assigned to user.");

            var service = _providerFactoryService.GetRequiredService(providerName);

            var result = await service.GetLatestRatesAsync(ratesRequest);
            return Ok(result);
        }

        [HttpPost("GetHistoricalExchangeRates")]
        public async Task<IActionResult> GetHistoricalExchangeRates([FromBody] HistoricalRequest ratesRequest)
        {
            var providerName = User.Claims.FirstOrDefault(c => c.Type == "currency_provider")?.Value;

            if (string.IsNullOrEmpty(providerName))
                return BadRequest("Currency provider not assigned to user.");

            var service = _providerFactoryService.GetRequiredService(providerName);

            var result = await service.GetHistoricalExchangeRates(ratesRequest);
            return Ok(result);
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
