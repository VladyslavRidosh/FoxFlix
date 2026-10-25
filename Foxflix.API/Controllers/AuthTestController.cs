using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/auth-test")]
public class AuthTestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok(new
        {
            message = "Цей endpoint доступний без авторизації."
        });
    }

    [Authorize]
    [HttpGet("private")]
    public IActionResult Private()
    {
        return Ok(new
        {
            message = "Авторизація успішна.",
            user = User.Identity?.Name
        });
    }
}