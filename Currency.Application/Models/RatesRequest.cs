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

    public class HistoricalRequest : CommonFields
    {
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public abstract class CommonFields
    {
        [SingleCurrencyCode] // 👈 strictly one code only
        public string BaseCurrency { get; set; } = string.Empty;

        [CurrencySymbols] // 👈 can be multiple codes
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

    public class CurrencySymbolsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                var codes = str.Split(',')
                               .Select(s => s.Trim())
                               .ToList();

                foreach (var code in codes)
                {
                    if (code.Length != 3 || !Regex.IsMatch(code, "^[A-Z]{3}$"))
                    {
                        // Force error key to "symbols"
                        return new ValidationResult(
                            $"symbols must contain only 3-letter uppercase currency codes separated by commas. Invalid: {code}",
                            new[] { "symbols" }
                        );
                    }
                }
            }

            return ValidationResult.Success!;
        }
    }

    /// <summary>
    /// Validates that a property contains exactly one 3-letter uppercase code.
    /// Used for baseCurrency.
    /// </summary>
    public class SingleCurrencyCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                if (str.Contains(",")) // ❌ no multiple codes
                {
                    return new ValidationResult(
                        $"{validationContext.MemberName} must contain exactly one 3-letter uppercase currency code (no commas)."
                    );
                }

                if (str.Length != 3 || !Regex.IsMatch(str, "^[A-Z]{3}$"))
                {
                    return new ValidationResult(
                        $"{validationContext.MemberName} must be a 3-letter uppercase currency code."
                    );
                }
            }

            return ValidationResult.Success!;
        }
    }

}
