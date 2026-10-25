using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Foxflix.Web.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;

    public AccountController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient("FoxflixAPI");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(
        LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response =
            await _httpClient.PostAsJsonAsync(
                "login",
                new
                {
                    email = model.Email,
                    password = model.Password
                });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Неправильний email або пароль.");

            return View(model);
        }

        var result =
            await response.Content
                .ReadFromJsonAsync<LoginResponse>();

        if (result is null ||
            string.IsNullOrWhiteSpace(result.AccessToken))
        {
            ModelState.AddModelError(
                string.Empty,
                "Не вдалося отримати токен.");

            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.Email,
                model.Email),

            new Claim(
                ClaimTypes.Name,
                model.Email),

            new Claim(
                "access_token",
                result.AccessToken)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme,
            principal);

        return RedirectToAction(
            "Index",
            "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var response = await _httpClient.PostAsJsonAsync(
        "register",
        new
        {
            email = model.Email,
            password = model.Password
        });

    if (!response.IsSuccessStatusCode)
    {
        ModelState.AddModelError(
            string.Empty,
            "Не вдалося створити акаунт. Перевір email і пароль.");

        return View(model);
    }

    return RedirectToAction("Login");
}

public class RegisterViewModel
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        return RedirectToAction(
            "Index",
            "Home");
    }

    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } =
            string.Empty;
    }
}