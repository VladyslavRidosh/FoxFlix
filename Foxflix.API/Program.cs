using Foxflix.Infrastructure.Identity;
using Foxflix.Infrastructure.Persistence.Context;
using Foxflix.Infrastructure.Services;
using Foxflix.API.Services;
using Foxflix.API.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("FoxflixWeb", policy =>
    {
        policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod();
    });
});


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "Bearer",
            In = ParameterLocation.Header,
            Description = "Введіть Bearer токен."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document)] = []
        });
});


var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");


if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' не знайдено.");
}


builder.Services.AddDbContext<FoxflixDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddDbContext<FoxflixIdentityDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<FoxflixIdentityDbContext>();

builder.Services.AddSingleton<
    IFileStorageService,
    FileStorageService>();

builder.Services.AddHttpClient<TmdbService>();

builder.WebHost.ConfigureKestrel(
    options =>
    {
        options.Limits.MaxRequestBodySize =
            5L * 1024 * 1024 * 1024;
    });


var app = builder.Build();

// Create any missing PostgreSQL tables before Identity performs its first query.
await DatabaseBootstrapper.InitializeAsync(app.Services);

using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

    await IdentitySeeder.SeedAsync(
        roleManager,
        userManager,
        builder.Configuration);
}

app.UseSwagger();

app.UseSwaggerUI();

app.UseRouting();

app.UseCors("FoxflixWeb");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapIdentityApi<ApplicationUser>();


app.Run();