using Microsoft.AspNetCore.Authentication.Cookies;
using Foxflix.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiAuthenticationHandler>();
builder.Services.AddHttpClient("FoxflixAPI", client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? "https://localhost:7283/";

    client.BaseAddress = new Uri(apiUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromMinutes(5);
}).AddHttpMessageHandler<ApiAuthenticationHandler>();

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();