CurrencyConverter
Introduction
CurrencyConverter is an ASP.NET Core based application that provides currency conversion services and exchange rate information. The solution is divided into several projects including a web front‑end with MVC views, a WebAPI secured by IdentityServer, and an application layer that manages currency conversion logic and caching. The application integrates with mulBelow is the complete README documentation for the repository:

------------------------------------------------------------

# CurrencyConverter

## Introduction

CurrencyConverter is an ASP.NET Core based application that provides currency conversion services and exchange rate information. The solution is divided into several projects including a web front‑end with MVC views, a WebAPI secured by IdentityServer, and an application layer that manages currency conversion logic and caching. The application integrates with multiple currency provider APIs (for example, “frankfurter” and “openexchange”) so users can obtain the latest rates, convert currencies, and even explore historical exchange data. The system is built on .NET 8 and uses industry‑standard components such as IdentityServer4 for authentication, Redis for caching, and Swagger for API documentation.

## Features

- **Currency Conversion:** Submit an amount and convert from one currency to another using supported providers. The conversion logic validates the input and rejects unsupported currencies such as TRY, PLN, THB, and MXN. fileciteturn0file16
- **Latest Rates:** Retrieve and display the most recent currency rates along with the associated date.
- **Historical Rates:** Query historical exchange rates over a specified date range.
- **Multiple Providers:** Supports at least two currency providers – one focused on the frankfurter API and another on openexchange.
- **Authentication & Authorization:** Secured API endpoints with IdentityServer4, JWT Bearer tokens, and cookie authentication for the web interface.
- **Swagger Integration:** Auto-generated Swagger user interface is available during development for testing endpoints.
- **Caching:** Redis integration to improve performance for recurring requests.
- **Responsive UI:** Modern web views built using Razor and Bootstrap ensure a user-friendly experience on different devices.

## Requirements

- **.NET 8 SDK:** Ensure that the latest .NET 8 SDK is installed to build and run the application.
- **Redis Server:** A running Redis instance is required for caching responses.
- **Identity Server Configuration:** The API security relies on IdentityServer4. You must provide valid client credentials and configure the token authority.
- **NuGet Dependencies:**  
  - IdentityServer4 and IdentityServer4.Storage  
  - Microsoft.AspNetCore.Authentication.JwtBearer  
  - Swashbuckle.AspNetCore  
  - Various OpenTelemetry, Polly, and Serilog packages  
  (See the respective project files for full details.) fileciteturn0file17

## Installation

1. **Clone the Repository:**

   Code example:
   ------------------------------------------------
   git clone 
   ------------------------------------------------

2. **Restore NuGet Packages:**

   Open a terminal at the solution root and run:
   ------------------------------------------------
   dotnet restore
   ------------------------------------------------

3. **Update Configuration Files:**

   - In *appsettings.json* (located under the WebAPI project), update the settings for IdentityServerConfig, RedisServerConfig, OpenExchangeRates (such as the AppId), and Token authority. 
   - In *Currency.Application/Constants/CurrencyConstants.cs*, note the excluded currencies that the application currently does not support.

4. **Build the Solution:**

   Run:
   ------------------------------------------------
   dotnet build
   ------------------------------------------------

## Usage

### Running the Application

- **Web API:**
  - Navigate to the WebAPI project (CurrencyConverter.WebApi) directory.
  - Run the project using:
    ------------------------------------------------
    dotnet run
    ------------------------------------------------
  - The Swagger UI is automatically launched when the environment is development (check *launchSettings.json* for the URL). 

- **Web Front‑end:**
  - The MVC project (CurrencyConverter) provides user interfaces for rates and currency conversion.
  - Open a browser and navigate to the default URL (e.g., https://localhost) to access the conversion view and historical rate pages.

### API Endpoints

- **Currency Rate Retrieval:** Endpoints such as `/Rates/GetLatestRatesAsync` and `/Rates/GetHistoricalExchangeRates` are available from the API secured by token authentication.
- **Currency Conversion:** The `/Rates/ConvertExchangeRates` endpoint accepts POST requests with conversion details.
- **Authentication:** Use the `/api/Login/GetToken` endpoint to generate an access token by posting a valid TokenRequest object. 

### UI Features

- **Latest Rates Page:** View the current conversion rates and the last updated date.
- **Currency Conversion Form:** Fill in the source currency, target currency, and amount. Validation ensures invalid inputs are rejected.
- **Historical Rates Page:** Enter a base currency and date range to retrieve the historical data in a tabular format.

## Configuration

The application is highly configurable. The key settings are available via the following files:

| File | Configuration Setting | Description |
|------|-----------------------|-------------|
| appsettings.json (WebAPI) | IdentityServerConfig and Token | Settings for client authentication, client secrets, and authority URLs |
| appsettings.json (WebAPI) | RedisServerConfig | Specify the Redis server address used for caching data |
| appsettings.json (WebAPI) | OpenExchangeRates | Provide the AppId required when using the OpenExchange currency provider |
| Currency.Application/Constants/CurrencyConstants.cs | ExcludedCurrencies | Contains a set of currencies for which conversion is not supported |

For example, in *appsettings.json*:
------------------------------------------------
{
  "IdentityServerConfig": {
    "ClientId": "my_client",
    "ClientSecret": "",
    "Scope": "currency_api",
    "Authority": "https://localhost:7248"
  },
  "RedisServerConfig": {
    "Authority": "localhost:6379"
  },
  "OpenExchangeRates": {
    "AppId": ""
  },
  "Token": {
    "Authority": "https://localhost:7248"
  }
}
------------------------------------------------

## Contributing

Contributions to CurrencyConverter are welcome. Here are the suggested steps for contributing:

- **Fork the Repository:** Create your own fork on GitHub.
- **Create a Branch:** Develop your feature or fix in a separate branch.
- **Commit Changes:** Ensure that your commits are concise and related to your work.
- **Testing:** The repository includes unit tests (see the CurrencyConverterTests project) to help verify that your changes do not break existing functionalities.
- **Pull Request:** Submit a pull request with a clear description of your updates.

Before submitting a pull request, please review the coding standards and project structure. Discussions on issues and enhancement requests are welcomed.

------------------------------------------------------------

With its robust architecture and integrated features, CurrencyConverter provides a seamless experience whether you prefer using its web interface or calling its secured APIs. Enjoy contributing and using the application!xchange data. The system is built on .NET 8 and uses industry‑standard components such as IdentityServer4 for authentication, Redis for caching, and Swagger for API documentation
