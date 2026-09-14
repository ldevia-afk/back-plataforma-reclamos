using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>
/// Selector de usuario simulado para poder probar el módulo sin login real.
/// Este controlador se reemplaza (o se elimina) cuando el módulo se integre
/// con la autenticación de la plataforma existente.
/// </summary>
public class SessionController : GestionCasosControllerBase
{
    private readonly ICatalogService _catalogService;

    public SessionController(ICurrentUserService currentUserService, ICatalogService catalogService) : base(currentUserService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Switch()
    {
        var vm = new SwitchUserViewModel
        {
            ClientUsers = _catalogService.GetUsers(Models.UserProfileType.Cliente),
            InternalUsers = _catalogService.GetUsers(Models.UserProfileType.Interno),
            AdminUsers = _catalogService.GetUsers()
                .Where(u => u.ProfileType is Models.UserProfileType.Administrador or Models.UserProfileType.AdministradorPais)
                .ToList(),
            CountryNamesById = _catalogService.GetCountries().ToDictionary(c => c.Id, c => c.Name)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetUser(int userId)
    {
        CurrentUserService.SetCurrentUser(userId);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        CurrentUserService.ClearCurrentUser();
        return RedirectToAction("Switch");
    }
}
