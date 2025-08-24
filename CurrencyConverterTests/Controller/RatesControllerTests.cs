using Currency.Application.Interfaces;
using Currency.Application.Interfaces.Redis;
using Currency.Application.Models;
using Currency.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Currency.WebApi.Tests.Controllers
{
    public class RatesControllerTests
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
        public async Task GetLatestRatesAsync_ShouldReturnCachedData_WhenCacheHit()
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
        public async Task GetLatestRatesAsync_ShouldCallProvider_WhenCacheMiss()
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
        public async Task GetLatestRatesAsync_ShouldReturnBadRequest_WhenNoProviderClaim()
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
    }
}
