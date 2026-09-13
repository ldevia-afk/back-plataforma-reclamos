using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>Sección del perfil Cliente: alta y seguimiento de sus propios casos.</summary>
public class CasesController : GestionCasosControllerBase
{
    private readonly ICaseService _caseService;
    private readonly ICatalogService _catalogService;

    public CasesController(ICurrentUserService currentUserService, ICaseService caseService, ICatalogService catalogService)
        : base(currentUserService)
    {
        _caseService = caseService;
        _catalogService = catalogService;
    }

    public IActionResult Index()
    {
        var guard = RequireProfile(UserProfileType.Cliente);
        if (guard != null) return guard;

        var user = CurrentUser!;
        var cases = _caseService.GetCasesForClientUser(user);
        var items = cases.Select(ToListItem).ToList();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var guard = RequireProfile(UserProfileType.Cliente);
        if (guard != null) return guard;

        return View(BuildCreateViewModel(CurrentUser!));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CaseCreateViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Cliente);
        if (guard != null) return guard;

        var user = CurrentUser!;

        if (!ModelState.IsValid || model.ClientId == null || !user.AssignedClientIds.Contains(model.ClientId.Value))
        {
            if (model.ClientId != null && !user.AssignedClientIds.Contains(model.ClientId.Value))
            {
                ModelState.AddModelError(string.Empty, "El cliente seleccionado no está asociado a tu usuario.");
            }
            var vm = BuildCreateViewModel(user);
            vm.ClientId = model.ClientId;
            vm.BranchIds = model.BranchIds;
            vm.CategoryId = model.CategoryId;
            vm.Description = model.Description;
            return View(vm);
        }

        var validBranchIds = _catalogService.GetBranches(clientId: model.ClientId.Value).Select(b => b.Id).ToHashSet();
        var branchIds = model.BranchIds.Where(validBranchIds.Contains).ToList();
        if (branchIds.Count == 0)
        {
            ModelState.AddModelError(nameof(model.BranchIds), "Seleccioná al menos una sucursal válida.");
            var vm = BuildCreateViewModel(user);
            vm.ClientId = model.ClientId;
            vm.CategoryId = model.CategoryId;
            vm.Description = model.Description;
            return View(vm);
        }

        var created = _caseService.CreateCases(user, model.ClientId.Value, branchIds, model.CategoryId!.Value, model.Description.Trim());
        TempData["Success"] = created.Count == 1
            ? $"Se registró el caso {created[0].Number}."
            : $"Se registraron {created.Count} casos (uno por sucursal): {string.Join(", ", created.Select(c => c.Number))}.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var guard = RequireProfile(UserProfileType.Cliente);
        if (guard != null) return guard;

        var user = CurrentUser!;
        var serviceCase = _caseService.GetCase(id);
        if (serviceCase == null || !user.AssignedClientIds.Contains(serviceCase.ClientId))
        {
            return NotFound();
        }

        return View(CasesSharedMapper.ToDetailViewModel(serviceCase, _catalogService, canManage: false));
    }

    private CaseCreateViewModel BuildCreateViewModel(AppUser user)
    {
        var clients = _catalogService.GetClients().Where(c => user.AssignedClientIds.Contains(c.Id)).ToList();
        return new CaseCreateViewModel
        {
            AvailableClients = clients.Select(c => new ClientWithBranchesOption
            {
                ClientId = c.Id,
                ClientName = c.Name,
                Branches = _catalogService.GetBranches(clientId: c.Id).Select(b => new BranchOption { Id = b.Id, Name = b.Name }).ToList()
            }).ToList(),
            AvailableCategories = _catalogService.GetCategories().Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList()
        };
    }

    private CaseListItemViewModel ToListItem(ServiceCase c) => CasesSharedMapper.ToListItem(c, _catalogService);
}
