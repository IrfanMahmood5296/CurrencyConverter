using Currency.Application.Helpers;
using Currency.Application.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Currency.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Username == "admin" && request.Password == "password")
            {
                var token = JwtTokenHelper.GenerateJwtToken(request.Username, _configuration);

                return Ok(new
                {
                    Username = request.Username,
                    Token = token
                });
            }

            return Unauthorized(new { error = "Invalid username or password" });
        }

        [AllowAnonymous]
        [HttpPost("GetToken")]
        public async Task<IActionResult> GetToken([FromBody] TokenRequest requestModel)
        {
            if (requestModel == null ||
            string.IsNullOrEmpty(requestModel.ClientId) || string.IsNullOrEmpty(requestModel.ClientSecret))
            {
                return BadRequest("ClientId, ClientSecret, and Scope are required.");
            }

            using var httpClient = new HttpClient();

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new("password", "password2"),
                new("username", "user2"),
                new("client_id", "my_client"),
                new("client_secret", "my_secret"),
                new("grant_type", "password"),
                new("scope", "currency_api")
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7248/connect/token")
            {
                Content = new FormUrlEncodedContent(keyValues)
            };

            var response = await httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            return Ok(tokenResponse);
        }
    }
}
