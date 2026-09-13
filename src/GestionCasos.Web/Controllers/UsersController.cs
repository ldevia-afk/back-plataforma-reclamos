using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>ABM de usuarios: perfil, país, clientes asignados (Cliente) o sucursales/grupos (Interno).</summary>
public class UsersController : GestionCasosControllerBase
{
    private readonly ICatalogService _catalogService;

    public UsersController(ICurrentUserService currentUserService, ICatalogService catalogService) : base(currentUserService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Index()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        ViewBag.CountryNames = _catalogService.GetCountries().ToDictionary(c => c.Id, c => c.Name);
        return View(_catalogService.GetUsers());
    }

    public IActionResult Create()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        return View("Form", BuildFormViewModel(new UserFormViewModel(), null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserFormViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model, model.CountryId));
        }

        _catalogService.CreateUser(MapToUser(model));
        TempData["Success"] = "Usuario creado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        var user = _catalogService.GetUser(id);
        if (user == null) return NotFound();

        var model = new UserFormViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            ProfileType = user.ProfileType,
            CountryId = user.CountryId,
            AssignedClientIds = user.AssignedClientIds,
            HasAllBranches = user.HasAllBranches,
            AssignedBranchIds = user.AssignedBranchIds,
            ResolverGroupIds = user.ResolverGroupIds
        };
        return View("Form", BuildFormViewModel(model, user.CountryId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UserFormViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model, model.CountryId));
        }

        _catalogService.UpdateUser(MapToUser(model));
        TempData["Success"] = "Usuario actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _catalogService.DeleteUser(id);
        TempData["Success"] = "Usuario eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private static AppUser MapToUser(UserFormViewModel model) => new()
    {
        Id = model.Id,
        Name = model.Name.Trim(),
        Email = model.Email.Trim(),
        ProfileType = model.ProfileType!.Value,
        CountryId = model.CountryId!.Value,
        AssignedClientIds = model.ProfileType == UserProfileType.Cliente ? model.AssignedClientIds : new List<int>(),
        HasAllBranches = model.ProfileType == UserProfileType.Interno && model.HasAllBranches,
        AssignedBranchIds = model.ProfileType == UserProfileType.Interno && !model.HasAllBranches ? model.AssignedBranchIds : new List<int>(),
        ResolverGroupIds = model.ProfileType == UserProfileType.Interno ? model.ResolverGroupIds : new List<int>()
    };

    private UserFormViewModel BuildFormViewModel(UserFormViewModel model, int? countryId)
    {
        model.AvailableCountries = _catalogService.GetCountries().Select(c => new CountryOption { Id = c.Id, Name = c.Name }).ToList();
        model.AvailableClients = _catalogService.GetClients();
        model.AvailableBranches = _catalogService.GetBranches();
        model.AvailableResolverGroups = _catalogService.GetResolverGroups();
        return model;
    }
}
