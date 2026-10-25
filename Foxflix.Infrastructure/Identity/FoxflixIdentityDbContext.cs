using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.Infrastructure.Identity;

public class FoxflixIdentityDbContext
    : IdentityDbContext<ApplicationUser>
{
    public FoxflixIdentityDbContext(
        DbContextOptions<FoxflixIdentityDbContext> options)
        : base(options)
    {
    }
}