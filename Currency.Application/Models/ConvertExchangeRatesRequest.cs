using Currency.Application.Constants;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Currency.Application.Models
{
    public class ConvertExchangeRatesRequest
    {
        [Required(ErrorMessage = "From currency is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
        public required string From { get; set; }

        [Required(ErrorMessage = "To currency is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
        public required string To { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public required decimal Amount { get; set; }

        [JsonIgnore] 
        public DateTime Date { get; set; }
        [JsonIgnore]
        public Dictionary<string, decimal> ConvertedAmounts { get; set; } = [];

        public bool IsValid(out string errorMessage)
        {
            if (CurrencyConstants.ExcludedCurrencies.Contains(From))
            {
                errorMessage = $"From currency '{From}' is not supported. {CurrencyConstants.ExcludedCurrencyErrorMessage}";
                return false;
            }

            if (CurrencyConstants.ExcludedCurrencies.Contains(To))
            {
                errorMessage = $"To currency '{To}' is not supported. {CurrencyConstants.ExcludedCurrencyErrorMessage}";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
