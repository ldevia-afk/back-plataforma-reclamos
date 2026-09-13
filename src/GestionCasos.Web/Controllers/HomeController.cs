using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

public class HomeController : GestionCasosControllerBase
{
    public HomeController(ICurrentUserService currentUserService) : base(currentUserService)
    {
    }

    public IActionResult Index()
    {
        var user = CurrentUser;
        if (user == null)
        {
            return RedirectToAction("Switch", "Session");
        }

        return user.ProfileType == UserProfileType.Cliente
            ? RedirectToAction("Index", "Cases")
            : RedirectToAction("Index", "InternalCases");
    }

    public IActionResult Error()
    {
        return View();
    }
}
