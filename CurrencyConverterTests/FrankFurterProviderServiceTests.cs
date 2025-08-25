using Currency.Application.Interfaces;
using Currency.Application.Interfaces.Redis;
using Currency.Application.Models;
using Currency.Application.Services;
using Currency.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;


namespace Currency.Tests
{
    public class FrankFurterProviderServiceTests
    {
        private readonly Mock<IProviderFactoryService> _mockProviderFactory = new();
        private readonly Mock<IRedisCacheService> _mockCache = new();
        private readonly Mock<ILogger<RatesController>> _mockLogger = new();

        private RatesController CreateControllerWithUser(string providerName = "frankfurter")
        {
            var controller = new RatesController(
                _mockProviderFactory.Object,
                _mockCache.Object,
                _mockLogger.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim("currency_provider", providerName)
                    }))
                }
            };

            return controller;
        }

        [Fact]
        private async Task GetLatestRatesAsync_ReturnsResponse_WhenDataIsAvailable()
        {
            // Arrange
            var ratesRequest = new RatesRequest
            {
                StartDate = new DateTime(2025, 08, 26),
                BaseCurrency = "USD",
                Symbols = "EUR,GBP"
            };

            var rateResponse = new RateResponse
            {
                BaseCurrency = "USD",
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.91m },
                    { "GBP", 0.79m }
                },

                Date = (DateTime)ratesRequest.StartDate
            };

            // Mock HttpClient
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(rateResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var loggerMock = new Mock<ILogger<FrankFurterProviderService>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            var service = new FrankFurterProviderService(
                httpClient,
                loggerMock.Object,
                httpContextAccessorMock.Object
            );

            // Act
            var result = await service.GetLatestRatesAsync(ratesRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ratesRequest.BaseCurrency, result.BaseCurrency);
            Assert.Equal(2, result.Rates!.Count);
            Assert.Equal(0.91m, result.Rates["EUR"]);
            Assert.Equal(0.79m, result.Rates["GBP"]);
            Assert.Equal(ratesRequest.StartDate, result.Date);
        }

        [Fact]
        private async Task GetLatestRatesAsync_ThrowsException_WhenResponseIsNull()
        {
            // Arrange
            var ratesRequest = new RatesRequest
            {

            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(new RateResponse()),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var loggerMock = new Mock<ILogger<FrankFurterProviderService>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            var service = new FrankFurterProviderService(
                httpClient,
                loggerMock.Object,
                httpContextAccessorMock.Object
            );

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.GetLatestRatesAsync(ratesRequest));
        }

        [Fact]
        private async Task GetLatestRatesAsync_ShouldReturnCachedData_WhenCacheHit()
        {
            // Arrange
            var cachedData = new RateResponse
            {
                BaseCurrency = "USD",
                Date = new DateTime(2024, 8, 23, 23, 34, 33),
                Rates = new Dictionary<string, decimal> { { "EUR", 0.92m } }
            };

            _mockCache.Setup(c => c.GetAsync<object>(It.IsAny<string>()))
                      .ReturnsAsync(cachedData);

            var controller = CreateControllerWithUser("frankfurter");
            var request = new RatesRequest { BaseCurrency = "USD", StartDate = new DateTime(2024, 8, 23, 23, 34, 33) };

            // Act
            var result = await controller.GetLatestRatesAsync(request);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            okResult.Value.Should().BeEquivalentTo(cachedData);

        }

        [Fact]
        private async Task GetLatestRatesAsync_ShouldCallProvider_WhenCacheMiss()
        {
            // Arrange
            var request = new RatesRequest { BaseCurrency = "USD", StartDate = new DateTime(2024, 8, 23, 23, 34, 33) };
            var cacheKey = "frankfurter:latest:USD__23/08/2024 11:34:33 pm";

            // Simulate cache miss
            _mockCache.Setup(c => c.GetAsync<RateResponse>(cacheKey))
                      .ReturnsAsync((RateResponse?)null);

            _mockCache.Setup(c => c.SetAsync(cacheKey, It.IsAny<RateResponse>(), It.IsAny<TimeSpan>()))
                      .Returns(Task.CompletedTask);

            // Mock provider
            var mockService = new Mock<ICurrencyProvider>();
            var providerResponse = new RateResponse
            {
                BaseCurrency = "USD",
                Date = new DateTime(2024, 8, 23, 23, 34, 33),
                Rates = new Dictionary<string, decimal> { { "EUR", 0.92m } }
            };

            mockService.Setup(p => p.GetLatestRatesAsync(It.IsAny<RatesRequest>()))
                       .ReturnsAsync(providerResponse);

            _mockProviderFactory.Setup(f => f.GetRequiredService("frankfurter"))
                                .Returns(mockService.Object);

            var controller = CreateControllerWithUser();

            // Act
            var result = await controller.GetLatestRatesAsync(request);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult!.Value as RateResponse;
            response.Should().NotBeNull();
            response!.Rates.Should().ContainKey("EUR");
            response.Rates["EUR"].Should().Be(0.92m);

            // Verify provider was called
            mockService.Verify(p => p.GetLatestRatesAsync(It.IsAny<RatesRequest>()), Times.Once);

            // Verify cache was set
            _mockCache.Verify(c => c.SetAsync(cacheKey, providerResponse, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        private async Task GetLatestRatesAsync_ShouldReturnBadRequest_WhenNoProviderClaim()
        {
            // Arrange
            var controller = CreateControllerWithUser(providerName: "");
            var request = new RatesRequest { BaseCurrency = "USD" };

            // Act
            var result = await controller.GetLatestRatesAsync(request);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().BeEquivalentTo(new
            {
                Error = "ProviderNotAssigned",
                Message = "Currency provider not assigned to user.",
                Timestamp = badRequest.Value.GetType().GetProperty("Timestamp")!.GetValue(badRequest.Value)
            });
        }

        [Fact]
        private async Task GetHistoricalExchangeRates()
        {
            // Arrange
            var ratesRequest = new HistoricalRequest
            {
                StartDate = new DateTime(2025, 08, 01),
                EndDate = new DateTime(2025, 08, 05),
                BaseCurrency = "USD",
                Symbols = "EUR,GBP",
                PageNumber = 1,
                PageSize = 2
            };

            var historicalRates = new Dictionary<string, Dictionary<string, decimal>>
            {
                { "2025-08-05", new Dictionary<string, decimal> { { "EUR", 0.91m }, { "GBP", 0.79m } } },
                { "2025-08-04", new Dictionary<string, decimal> { { "EUR", 0.92m }, { "GBP", 0.80m } } },
                { "2025-08-03", new Dictionary<string, decimal> { { "EUR", 0.90m }, { "GBP", 0.78m } } },
            };

            var historicalResponse = new HistoricalRateResponse
            {
                BaseCurrency = "USD",
                HistoricalRates = historicalRates
            };

            // Mock HttpClient
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(historicalResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var loggerMock = new Mock<ILogger<FrankFurterProviderService>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var service = new FrankFurterProviderService(httpClient, loggerMock.Object, httpContextAccessorMock.Object);

            // Act
            var result = await service.GetHistoricalExchangeRates(ratesRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ratesRequest.PageNumber, result.Page);
            Assert.Equal(ratesRequest.PageSize, result.PageSize);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(2, result.TotalPages); // 3 items / 2 per page
            Assert.Equal(2, result.items.Count); // page size
            Assert.Equal(new DateTime(2025, 08, 05), result.items[0].Date);
        }
    }
}
