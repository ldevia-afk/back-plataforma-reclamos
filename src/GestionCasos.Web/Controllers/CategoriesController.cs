using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>ABM de categorías internas para clasificar casos ya recibidos (perfiles Administrador / Administrador de país).</summary>
public class CategoriesController : GestionCasosControllerBase
{
    private readonly ICatalogService _catalogService;

    public CategoriesController(ICurrentUserService currentUserService, ICatalogService catalogService) : base(currentUserService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Index(string? q, int? page, int? pageSize)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var countryScope = ManagementCountryScope;
        var countryNames = _catalogService.GetCountries().ToDictionary(c => c.Id, c => c.Name);

        var items = _catalogService.GetCategories(countryScope)
            .Select(c => new CategoryListItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                CountryName = countryNames.TryGetValue(c.CountryId, out var cn) ? cn : string.Empty
            })
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            items = items.Where(c =>
                c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.CountryName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var vm = new CategoryListViewModel
        {
            Q = q,
            Paging = items.ToPagedResult(page, pageSize),
            CanChooseCountry = countryScope == null,
            Form = new CategoryFormViewModel
            {
                CountryId = countryScope,
                AvailableCountries = _catalogService.GetCountries().Select(c => new CountryOption { Id = c.Id, Name = c.Name }).ToList()
            }
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CategoryFormViewModel model)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        // Administrador de país: el país queda fijo en el suyo, sin importar lo que llegue del formulario.
        var countryId = ManagementCountryScope ?? model.CountryId;

        if (string.IsNullOrWhiteSpace(model.Name) || countryId == null)
        {
            TempData["Error"] = "Ingresá un nombre y un país válidos para la categoría.";
            return RedirectToAction(nameof(Index));
        }

        _catalogService.CreateCategory(model.Name.Trim(), countryId.Value);
        TempData["Success"] = "Categoría creada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var category = _catalogService.GetCategory(id);
        if (category == null || (ManagementCountryScope is { } scope && category.CountryId != scope))
        {
            TempData["Error"] = "No tenés acceso a esa categoría.";
            return RedirectToAction(nameof(Index));
        }

        _catalogService.DeleteCategory(id);
        TempData["Success"] = "Categoría eliminada.";
        return RedirectToAction(nameof(Index));
    }
}
