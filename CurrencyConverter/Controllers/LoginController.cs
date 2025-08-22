using Currency.Application.Helpers;
using Currency.Application.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CurrencyConverter.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;

        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("CurrencyApi");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("Login"); 
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("Login", new { Username = username, Password = password });

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (data != null)
                {
                    HttpContext.Session.SetString("JwtToken", data.Token);
                    var user = data.Username;
                }
                

                return RedirectToAction("Latest", "Rates");
            }

            ViewBag.Error = "Invalid login";
            return View("Login");
        }
    }
}
