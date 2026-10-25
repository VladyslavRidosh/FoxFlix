using Microsoft.AspNetCore.Mvc;

namespace Foxflix.Web.Controllers;

public class ProfileController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}