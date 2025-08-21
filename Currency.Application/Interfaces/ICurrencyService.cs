using Currency.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Interfaces
{
    public interface ICurrencyService
    {
        Task<RateResponse> GetLatestRatesAsync(string? baseCurrency, string? symbols = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> ConvertCurrencyAsync(string from, string to, decimal amount);
    }
}
