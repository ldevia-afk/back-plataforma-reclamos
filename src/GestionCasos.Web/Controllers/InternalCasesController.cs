using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>Bandeja del perfil Interno: recibir, agrupar/filtrar, derivar a grupo resolutor y cambiar estado.</summary>
public class InternalCasesController : GestionCasosControllerBase
{
    private readonly ICaseService _caseService;
    private readonly ICatalogService _catalogService;

    public InternalCasesController(ICurrentUserService currentUserService, ICaseService caseService, ICatalogService catalogService)
        : base(currentUserService)
    {
        _caseService = caseService;
        _catalogService = catalogService;
    }

    public IActionResult Index(int? filterCategoryId, CaseStatus? filterStatus, int? filterGroupId, bool groupByCategory = true)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        var categoryId = filterCategoryId;
        var status = filterStatus;
        var groupId = filterGroupId;

        var user = CurrentUser!;
        var cases = _caseService.GetCasesForInternalUser(user).AsEnumerable();

        if (categoryId.HasValue) cases = cases.Where(c => c.CategoryId == categoryId.Value);
        if (status.HasValue) cases = cases.Where(c => c.Status == status.Value);
        if (groupId.HasValue) cases = cases.Where(c => c.AssignedGroupId == groupId.Value);

        var items = cases.Select(c => CasesSharedMapper.ToListItem(c, _catalogService)).ToList();

        var vm = new InternalInboxViewModel
        {
            AllCases = items,
            GroupedByCategory = groupByCategory
                ? items.GroupBy(i => i.CategoryName).OrderBy(g => g.Key).ToList()
                : new List<IGrouping<string, CaseListItemViewModel>>(),
            Categories = _catalogService.GetCategories().Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList(),
            ResolverGroups = _catalogService.GetResolverGroups(countryId: user.CountryId).Select(g => new ResolverGroupOption { Id = g.Id, Name = g.Name }).ToList(),
            FilterCategoryId = categoryId,
            FilterStatus = status,
            FilterGroupId = groupId,
            GroupByCategory = groupByCategory
        };

        return View(vm);
    }

    public IActionResult Details(int id)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        var user = CurrentUser!;
        var serviceCase = _caseService.GetCase(id);
        if (serviceCase == null || serviceCase.CountryId != user.CountryId)
        {
            return NotFound();
        }
        if (!user.HasAllBranches && !user.AssignedBranchIds.Contains(serviceCase.BranchId))
        {
            return NotFound();
        }

        return View(CasesSharedMapper.ToDetailViewModel(serviceCase, _catalogService, canManage: true));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AssignGroup(int id, int? resolverGroupId, string? comment)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _caseService.AssignGroup(id, resolverGroupId, CurrentUser!.Id, comment);
        TempData["Success"] = "Se actualizó el grupo resolutor del caso.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangeStatus(int id, CaseStatus newStatus)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _caseService.ChangeStatus(id, newStatus, CurrentUser!.Id);
        TempData["Success"] = "Se actualizó el estado del caso.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
