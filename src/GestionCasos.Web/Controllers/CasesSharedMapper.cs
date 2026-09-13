using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;

namespace GestionCasos.Web.Controllers;

/// <summary>Mapeo de ServiceCase a los view models usados tanto por el perfil cliente como el interno.</summary>
public static class CasesSharedMapper
{
    public static CaseListItemViewModel ToListItem(ServiceCase c, ICatalogService catalog)
    {
        var client = catalog.GetClient(c.ClientId);
        var branch = catalog.GetBranch(c.BranchId);
        var category = c.CategoryId.HasValue ? catalog.GetCategory(c.CategoryId.Value) : null;
        var group = c.AssignedGroupId.HasValue ? catalog.GetResolverGroup(c.AssignedGroupId.Value) : null;

        return new CaseListItemViewModel
        {
            Id = c.Id,
            Number = c.Number,
            ClientName = client?.Name ?? "(cliente eliminado)",
            BranchName = branch?.Name ?? "(sucursal eliminada)",
            Type = c.Type,
            CategoryName = category?.Name,
            Status = c.Status,
            ResolverGroupName = group?.Name,
            CreatedAtUtc = c.CreatedAtUtc,
            Description = c.Description
        };
    }

    public static CaseDetailViewModel ToDetailViewModel(ServiceCase c, ICatalogService catalog, bool canManage)
    {
        var client = catalog.GetClient(c.ClientId);
        var country = catalog.GetCountry(c.CountryId);
        var category = c.CategoryId.HasValue ? catalog.GetCategory(c.CategoryId.Value) : null;
        var group = c.AssignedGroupId.HasValue ? catalog.GetResolverGroup(c.AssignedGroupId.Value) : null;
        var creator = catalog.GetUser(c.CreatedByUserId);

        return new CaseDetailViewModel
        {
            Id = c.Id,
            Number = c.Number,
            ClientName = client?.Name ?? "(cliente eliminado)",
            CountryName = country?.Name ?? string.Empty,
            BranchName = catalog.GetBranch(c.BranchId)?.Name ?? "(sucursal eliminada)",
            Type = c.Type,
            CategoryId = c.CategoryId,
            CategoryName = category?.Name,
            Description = c.Description,
            Status = c.Status,
            CreatedByName = creator?.Name ?? "(usuario eliminado)",
            CreatedAtUtc = c.CreatedAtUtc,
            ResolverGroupName = group?.Name,
            CanManage = canManage,
            AvailableGroups = canManage
                ? catalog.GetResolverGroups(countryId: c.CountryId).Select(g => new ResolverGroupOption { Id = g.Id, Name = g.Name }).ToList()
                : new List<ResolverGroupOption>(),
            AvailableCategories = canManage
                ? catalog.GetCategories().Select(cat => new CategoryOption { Id = cat.Id, Name = cat.Name }).ToList()
                : new List<CategoryOption>(),
            ResolvedAtUtc = c.StatusHistory.LastOrDefault(h => h.Status == CaseStatus.Resuelto)?.ChangedAtUtc,
            ResolutionComment = c.ResolutionComment,
            ResolutionConfirmed = c.ResolutionConfirmed,
            ResolutionConfirmedAtUtc = c.ResolutionConfirmedAtUtc,
            AssignedToHelpDesk = group?.IsHelpDesk ?? false,
            StatusHistory = c.StatusHistory
                .OrderBy(h => h.ChangedAtUtc)
                .Select(h => new StatusHistoryRow
                {
                    Status = h.Status,
                    ChangedAtUtc = h.ChangedAtUtc,
                    ChangedByName = h.ChangedByUserId.HasValue ? catalog.GetUser(h.ChangedByUserId.Value)?.Name : null
                })
                .ToList(),
            GroupHistory = c.GroupHistory
                .OrderBy(h => h.ChangedAtUtc)
                .Select(h => new GroupHistoryRow
                {
                    GroupName = h.ResolverGroupId.HasValue ? (catalog.GetResolverGroup(h.ResolverGroupId.Value)?.Name ?? "(grupo eliminado)") : "Sin grupo",
                    ChangedAtUtc = h.ChangedAtUtc,
                    ChangedByName = h.ChangedByUserId.HasValue ? catalog.GetUser(h.ChangedByUserId.Value)?.Name : null,
                    Comment = h.Comment
                })
                .ToList()
        };
    }
}
