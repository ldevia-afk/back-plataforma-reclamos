using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>ABM de grupos resolutores y sus miembros (perfil Interno).</summary>
public class ResolverGroupsController : GestionCasosControllerBase
{
    private readonly ICatalogService _catalogService;

    public ResolverGroupsController(ICurrentUserService currentUserService, ICatalogService catalogService) : base(currentUserService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Index()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        ViewBag.CountryNames = _catalogService.GetCountries().ToDictionary(c => c.Id, c => c.Name);
        return View(_catalogService.GetResolverGroups());
    }

    public IActionResult Create()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        return View("Form", BuildFormViewModel(new ResolverGroupFormViewModel { CountryId = CurrentUser!.CountryId }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ResolverGroupFormViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model));
        }

        _catalogService.CreateResolverGroup(model.Name.Trim(), model.CountryId!.Value, model.MemberUserIds);
        TempData["Success"] = "Grupo resolutor creado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        var group = _catalogService.GetResolverGroup(id);
        if (group == null) return NotFound();

        var model = new ResolverGroupFormViewModel
        {
            Id = group.Id,
            Name = group.Name,
            CountryId = group.CountryId,
            MemberUserIds = group.MemberUserIds
        };
        return View("Form", BuildFormViewModel(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ResolverGroupFormViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model));
        }

        _catalogService.UpdateResolverGroup(model.Id, model.Name.Trim(), model.MemberUserIds);
        TempData["Success"] = "Grupo resolutor actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _catalogService.DeleteResolverGroup(id);
        TempData["Success"] = "Grupo resolutor eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private ResolverGroupFormViewModel BuildFormViewModel(ResolverGroupFormViewModel model)
    {
        model.AvailableCountries = _catalogService.GetCountries().Select(c => new CountryOption { Id = c.Id, Name = c.Name }).ToList();
        model.AvailableUsers = _catalogService.GetUsers(UserProfileType.Interno);
        return model;
    }
}
