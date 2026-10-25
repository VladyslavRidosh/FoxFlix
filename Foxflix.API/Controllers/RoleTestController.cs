using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/role-test")]
public class RoleTestController : ControllerBase
{
    [Authorize(Roles = "User")]
    [HttpGet("user")]
    public IActionResult CheckUserRole()
    {
        return Ok(new
        {
            message = "Роль User підтверджена.",
            user = User.Identity?.Name,
            role = "User"
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult CheckAdminRole()
    {
        return Ok(new
        {
            message = "Роль Admin підтверджена.",
            user = User.Identity?.Name,
            role = "Admin"
        });
    }
}