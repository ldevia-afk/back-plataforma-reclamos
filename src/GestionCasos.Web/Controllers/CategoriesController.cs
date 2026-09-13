using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>ABM de categorías internas para clasificar casos ya recibidos (sólo perfil Interno).</summary>
public class CategoriesController : GestionCasosControllerBase
{
    private readonly ICatalogService _catalogService;

    public CategoriesController(ICurrentUserService currentUserService, ICatalogService catalogService) : base(currentUserService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Index()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        return View(_catalogService.GetCategories());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CategoryFormViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Ingresá un nombre válido para la categoría.";
            return RedirectToAction(nameof(Index));
        }

        _catalogService.CreateCategory(model.Name.Trim());
        TempData["Success"] = "Categoría creada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _catalogService.DeleteCategory(id);
        TempData["Success"] = "Categoría eliminada.";
        return RedirectToAction(nameof(Index));
    }
}
