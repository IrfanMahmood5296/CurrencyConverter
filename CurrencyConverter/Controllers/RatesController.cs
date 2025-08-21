using Currency.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Web.Controllers
{
    [Authorize]
    [Route("Rates")]
    public class RatesController : Controller
    {
        private readonly ICurrencyService _currencyService;
        public RatesController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetLatestRatesAsync")]
        //[Authorize(Roles = "reader,admin")]
        public async Task<IActionResult> GetLatestRatesAsync(string? baseCurrency, string? symbols = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = await _currencyService.GetLatestRatesAsync(baseCurrency, symbols, startDate, endDate);
            return View("Latest", result);
        }

        [HttpGet("convert")]
        public async Task<IActionResult> Convert([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount)
        {
            try
            {
                var result = await _currencyService.ConvertCurrencyAsync(from, to, amount);
                return Ok(new
                {
                    From = from,
                    To = to,
                    Amount = amount,
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