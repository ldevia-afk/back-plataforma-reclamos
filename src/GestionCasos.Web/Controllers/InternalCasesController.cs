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

    public IActionResult Index(
        string? q, CaseType? filterType, int? filterCategoryId, CaseStatus? filterStatus, int? filterGroupId,
        DateOnly? dateFrom, DateOnly? dateTo, bool groupByType = true, int? page = null, int? pageSize = null)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        var user = CurrentUser!;
        var cases = _caseService.GetCasesForInternalUser(user).AsEnumerable();

        if (filterType.HasValue) cases = cases.Where(c => c.Type == filterType.Value);
        if (filterCategoryId.HasValue) cases = cases.Where(c => c.CategoryId == filterCategoryId.Value);
        if (filterStatus.HasValue) cases = cases.Where(c => c.Status == filterStatus.Value);
        if (filterGroupId.HasValue) cases = cases.Where(c => c.AssignedGroupId == filterGroupId.Value);
        // Filtro por fecha de registro del caso (CreatedAtUtc), en la zona horaria local.
        if (dateFrom.HasValue) cases = cases.Where(c => DateOnly.FromDateTime(c.CreatedAtUtc.ToLocalTime()) >= dateFrom.Value);
        if (dateTo.HasValue) cases = cases.Where(c => DateOnly.FromDateTime(c.CreatedAtUtc.ToLocalTime()) <= dateTo.Value);

        var items = cases.Select(c => CasesSharedMapper.ToListItem(c, _catalogService)).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            items = items.Where(c =>
                c.Number.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.ClientName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.BranchName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var itemList = items.ToList();

        var vm = new InternalInboxViewModel
        {
            Q = q,
            Categories = _catalogService.GetCategories().Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList(),
            ResolverGroups = _catalogService.GetResolverGroups(countryId: user.CountryId).Select(g => new ResolverGroupOption { Id = g.Id, Name = g.Name }).ToList(),
            FilterType = filterType,
            FilterCategoryId = filterCategoryId,
            FilterStatus = filterStatus,
            FilterGroupId = filterGroupId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            GroupByType = groupByType
        };

        if (groupByType)
        {
            // La vista agrupada es un reporte de todo lo que matchea el filtro: no pagina.
            vm.AllCases = itemList;
            vm.GroupedByType = itemList.GroupBy(i => i.Type.ToDisplayName()).OrderBy(g => g.Key).ToList();
        }
        else
        {
            vm.Paging = itemList.ToPagedResult(page, pageSize);
            vm.AllCases = vm.Paging.Items;
        }

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
    public IActionResult AssignCategory(int id, int? categoryId)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _caseService.AssignCategory(id, categoryId);
        TempData["Success"] = "Se actualizó la categoría del caso.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangeStatus(int id, CaseStatus newStatus, string? resolutionComment)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (newStatus == CaseStatus.Resuelto && string.IsNullOrWhiteSpace(resolutionComment))
        {
            TempData["Error"] = "Para marcar el caso como Resuelto tenés que agregar un comentario con el detalle de la resolución.";
            return RedirectToAction(nameof(Details), new { id });
        }

        _caseService.ChangeStatus(id, newStatus, CurrentUser!.Id, resolutionComment);
        TempData["Success"] = newStatus == CaseStatus.Resuelto
            ? "Caso marcado como Resuelto. Se derivó a Mesa de Ayuda para confirmar el mensaje al cliente."
            : "Se actualizó el estado del caso.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BulkChangeStatus(List<int>? caseIds, CaseStatus newStatus, string? resolutionComment, string? returnUrl)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        IActionResult BackToInbox() => Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Index));

        if (caseIds == null || caseIds.Count == 0)
        {
            TempData["Error"] = "Seleccioná al menos un caso.";
            return BackToInbox();
        }

        if (newStatus == CaseStatus.Resuelto && string.IsNullOrWhiteSpace(resolutionComment))
        {
            TempData["Error"] = "Para marcar los casos como Resuelto tenés que agregar un comentario con el detalle de la resolución.";
            return BackToInbox();
        }

        var user = CurrentUser!;
        // Sólo se actualizan los casos que están dentro del alcance del usuario (mismo país / sucursales asignadas),
        // aunque el formulario haya llegado con otros ids (por ejemplo, manipulados a mano).
        var allowedIds = _caseService.GetCasesForInternalUser(user).Select(c => c.Id).ToHashSet();
        var idsToUpdate = caseIds.Where(allowedIds.Contains).Distinct().ToList();

        foreach (var id in idsToUpdate)
        {
            _caseService.ChangeStatus(id, newStatus, user.Id, resolutionComment);
        }

        TempData["Success"] = idsToUpdate.Count == 1
            ? $"Se actualizó 1 caso a estado {newStatus.ToDisplayName()}."
            : $"Se actualizaron {idsToUpdate.Count} casos a estado {newStatus.ToDisplayName()}.";
        return BackToInbox();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmResolution(int id, string clientMessage)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        if (string.IsNullOrWhiteSpace(clientMessage))
        {
            TempData["Error"] = "El mensaje para el cliente no puede estar vacío.";
            return RedirectToAction(nameof(Details), new { id });
        }

        _caseService.ConfirmResolution(id, clientMessage, CurrentUser!.Id);
        TempData["Success"] = "Se guardó y envió el mensaje de resolución al cliente.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
