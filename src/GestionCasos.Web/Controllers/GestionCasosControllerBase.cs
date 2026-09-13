using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>
/// Controlador base: resuelve el usuario "actual" (ver <see cref="ICurrentUserService"/>)
/// y ofrece un helper para exigir un perfil determinado antes de ejecutar una acción.
/// </summary>
public abstract class GestionCasosControllerBase : Controller
{
    protected readonly ICurrentUserService CurrentUserService;

    protected GestionCasosControllerBase(ICurrentUserService currentUserService)
    {
        CurrentUserService = currentUserService;
    }

    protected AppUser? CurrentUser => CurrentUserService.GetCurrentUser();

    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        ViewBag.CurrentUser = CurrentUser;
        base.OnActionExecuting(context);
    }

    /// <summary>Redirige a /Session/Switch si no hay usuario simulado elegido, o al Home si el perfil no coincide.</summary>
    protected IActionResult? RequireProfile(UserProfileType profileType)
    {
        var user = CurrentUser;
        if (user == null)
        {
            return RedirectToAction("Switch", "Session");
        }
        if (user.ProfileType != profileType)
        {
            TempData["Error"] = "No tenés acceso a esa sección con el perfil activo.";
            return RedirectToAction("Index", "Home");
        }
        return null;
    }
}
