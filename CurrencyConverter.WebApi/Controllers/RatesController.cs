using Currency.Application.Interfaces;
using Currency.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Currency.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        public RatesController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpPost("GetLatestRatesAsync")]
        public async Task<IActionResult> GetLatestRatesAsync([FromBody] RatesRequest ratesRequest)
        {
            var result = await _currencyService.GetLatestRatesAsync(ratesRequest);
            return Ok(result);
        }

        [HttpPost("GetHistoricalExchangeRates")]
        public async Task<IActionResult> GetHistoricalExchangeRates([FromBody] HistoricalRequest ratesRequest)
        {
            var result = await _currencyService.GetHistoricalExchangeRates(ratesRequest);
            return Ok(result);
        }

        [HttpPost("ConvertExchangeRates")]
        public async Task<IActionResult> Convert([FromBody] ConvertExchangeRatesRequest ratesRequest)
        {
            try
            {
                var result = await _currencyService.ConvertCurrencyAsync(ratesRequest);
                return Ok(new
                {
                    ConvertedAmount = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
