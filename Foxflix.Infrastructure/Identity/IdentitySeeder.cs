using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;

namespace Foxflix.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        string[] roles = ["User", "Moderator", "Admin"];

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(error => error.Description)));
            }
        }

        // Never ship a well-known admin password in source code.
        // Configure InitialAdmin__Email and InitialAdmin__Password to seed one.
        var adminEmail = configuration["InitialAdmin:Email"];
        var adminPassword = configuration["InitialAdmin:Password"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DisplayName = "Foxflix Admin"
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Не вдалося створити початкового адміністратора: " +
                    string.Join("; ", createResult.Errors.Select(error => error.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Не вдалося призначити роль Admin: " +
                    string.Join("; ", roleResult.Errors.Select(error => error.Description)));
            }
        }
    }
}
