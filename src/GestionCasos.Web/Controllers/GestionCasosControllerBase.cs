using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
    protected IActionResult? RequireProfile(UserProfileType profileType) => RequireAnyProfile(profileType);

    /// <summary>Igual que <see cref="RequireProfile"/>, pero acepta cualquiera de varios perfiles.</summary>
    protected IActionResult? RequireAnyProfile(params UserProfileType[] profileTypes)
    {
        var user = CurrentUser;
        if (user == null)
        {
            return RedirectToAction("Switch", "Session");
        }
        if (!profileTypes.Contains(user.ProfileType))
        {
            TempData["Error"] = "No tenés acceso a esa sección con el perfil activo.";
            return RedirectToAction("Index", "Home");
        }
        return null;
    }

    /// <summary>
    /// País al que hay que restringir la vista/gestión de datos maestros (usuarios, clientes,
    /// sucursales, categorías) para el usuario actual: su propio país para Administrador de país,
    /// o null (sin restricción, ve/administra todos los países) para Administrador global.
    /// </summary>
    protected int? ManagementCountryScope => CurrentUser!.ProfileType == UserProfileType.AdministradorPais ? CurrentUser!.CountryId : (int?)null;
}
