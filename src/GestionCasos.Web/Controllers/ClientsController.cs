using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>ABM de clientes y sus sucursales/puntos (perfil Interno).</summary>
public class ClientsController : GestionCasosControllerBase
{
    private readonly ICatalogService _catalogService;

    public ClientsController(ICurrentUserService currentUserService, ICatalogService catalogService) : base(currentUserService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Index()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        var clients = _catalogService.GetClients();
        ViewBag.CountryNames = _catalogService.GetCountries().ToDictionary(c => c.Id, c => c.Name);
        ViewBag.BranchCounts = clients.ToDictionary(c => c.Id, c => _catalogService.GetBranches(clientId: c.Id).Count);
        return View(clients);
    }

    public IActionResult Create()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        return View("Form", BuildFormViewModel(new ClientFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ClientFormViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model));
        }

        var client = _catalogService.CreateClient(model.Name.Trim(), model.CountryId!.Value);
        TempData["Success"] = "Cliente creado. Ahora podés agregarle sucursales.";
        return RedirectToAction(nameof(Edit), new { id = client.Id });
    }

    public IActionResult Edit(int id)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        var client = _catalogService.GetClient(id);
        if (client == null) return NotFound();

        var model = new ClientFormViewModel { Id = client.Id, Name = client.Name, CountryId = client.CountryId };
        return View("Form", BuildFormViewModel(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ClientFormViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model));
        }

        _catalogService.UpdateClient(model.Id, model.Name.Trim(), model.CountryId!.Value);
        TempData["Success"] = "Cliente actualizado.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _catalogService.DeleteClient(id);
        TempData["Success"] = "Cliente eliminado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddBranch(int clientId, string newBranchName)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (!string.IsNullOrWhiteSpace(newBranchName))
        {
            _catalogService.CreateBranch(newBranchName.Trim(), clientId);
            TempData["Success"] = "Sucursal agregada.";
        }
        return RedirectToAction(nameof(Edit), new { id = clientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteBranch(int branchId, int clientId)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _catalogService.DeleteBranch(branchId);
        TempData["Success"] = "Sucursal eliminada.";
        return RedirectToAction(nameof(Edit), new { id = clientId });
    }

    private ClientFormViewModel BuildFormViewModel(ClientFormViewModel model)
    {
        model.AvailableCountries = _catalogService.GetCountries().Select(c => new CountryOption { Id = c.Id, Name = c.Name }).ToList();
        if (model.Id != 0)
        {
            model.Branches = _catalogService.GetBranches(clientId: model.Id);
        }
        return model;
    }
}
