using Currency.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Interfaces
{
    public interface ICurrencyProvider
    {
        Task<RateResponse> GetLatestRatesAsync(RatesRequest ratesRequest);
        Task<HistoricalRateResponse> GetHistoricalExchangeRates(HistoricalRequest ratesRequest);
        Task<ConvertExchangeRatesRequest> ConvertCurrencyAsync(ConvertExchangeRatesRequest ratesRequest);
    }
}
