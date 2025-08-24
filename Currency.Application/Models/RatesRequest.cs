using Currency.Application.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Currency.Application.Models
{
    public class RatesRequest : CommonFields
    {
        public DateTime? StartDate { get; set; } = null;
    }

    public class HistoricalRequest: CommonFields
    {

        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
       
    }

    public abstract class CommonFields
    {
        [OptionalCurrencyCode]
        public string? BaseCurrency { get; set; } = null;

        [OptionalCurrencyCode]
        public string? Symbols { get; set; }

        public virtual bool IsValid(out string errorMessage)
        {
            if (CurrencyConstants.ExcludedCurrencies.Contains(BaseCurrency.ToUpperInvariant()))
            {
                errorMessage = $"Base currency '{BaseCurrency}' is not supported. {CurrencyConstants.ExcludedCurrencyErrorMessage}";
                return false;
            }

            if (!string.IsNullOrEmpty(Symbols))
            {
                var excludedSymbols = Symbols.Split(',')
                    .Select(s => s.Trim().ToUpper())
                    .Where(CurrencyConstants.ExcludedCurrencies.Contains)
                    .ToArray();

                if (excludedSymbols.Any())
                {
                    errorMessage = $"Target currencies '{string.Join(", ", excludedSymbols)}' are not supported. {CurrencyConstants.ExcludedCurrencyErrorMessage}";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }
    }

    public class OptionalCurrencyCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                if (str.Length != 3 || !Regex.IsMatch(str, "^[A-Z]{3}$"))
                {
                    return new ValidationResult($"{validationContext.MemberName} must be 3 uppercase letters");
                }
            }

            return ValidationResult.Success;
        }
    }

}
