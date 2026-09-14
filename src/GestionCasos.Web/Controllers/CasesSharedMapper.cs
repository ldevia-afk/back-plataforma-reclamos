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
            Description = c.Description,
            ResolutionConfirmed = c.ResolutionConfirmed,
            ResolutionComment = c.ResolutionComment
        };
    }

    public static CaseDetailViewModel ToDetailViewModel(ServiceCase c, ICatalogService catalog, bool canManage)
    {
        var client = catalog.GetClient(c.ClientId);
        var country = catalog.GetCountry(c.CountryId);
        var category = c.CategoryId.HasValue ? catalog.GetCategory(c.CategoryId.Value) : null;
        var group = c.AssignedGroupId.HasValue ? catalog.GetResolverGroup(c.AssignedGroupId.Value) : null;
        var creator = catalog.GetUser(c.CreatedByUserId);
        var assignedUser = c.AssignedUserId.HasValue ? catalog.GetUser(c.AssignedUserId.Value) : null;

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
            ResolverGroupId = c.AssignedGroupId,
            ResolverGroupName = group?.Name,
            AssignedUserId = c.AssignedUserId,
            AssignedUserName = assignedUser?.Name,
            CanManage = canManage,
            AvailableGroups = canManage
                ? catalog.GetResolverGroups(countryId: c.CountryId).Select(g => new ResolverGroupOption
                {
                    Id = g.Id,
                    Name = g.Name,
                    Members = g.MemberUserIds
                        .Select(catalog.GetUser)
                        .Where(u => u != null)
                        .Select(u => new UserOption { Id = u!.Id, Name = u.Name })
                        .ToList()
                }).ToList()
                : new List<ResolverGroupOption>(),
            AvailableCategories = canManage
                ? catalog.GetCategories().Select(cat => new CategoryOption { Id = cat.Id, Name = cat.Name }).ToList()
                : new List<CategoryOption>(),
            ResolvedAtUtc = c.History.LastOrDefault(h => h.EventType == CaseEventType.StatusChanged && h.Status == CaseStatus.Resuelto)?.OccurredAtUtc,
            ResolutionComment = c.ResolutionComment,
            ResolutionConfirmed = c.ResolutionConfirmed,
            ResolutionConfirmedAtUtc = c.ResolutionConfirmedAtUtc,
            AssignedToHelpDesk = group?.IsHelpDesk ?? false,
            History = c.History
                .OrderBy(h => h.OccurredAtUtc)
                .Select(h => ToHistoryRow(h, catalog, canManage))
                .ToList()
        };
    }

    private static CaseHistoryRow ToHistoryRow(CaseHistoryEntry h, ICatalogService catalog, bool canManage)
    {
        var changedByUser = h.ChangedByUserId.HasValue ? catalog.GetUser(h.ChangedByUserId.Value) : null;
        string? groupNames = null;
        if (changedByUser != null && changedByUser.ResolverGroupIds.Count > 0)
        {
            groupNames = string.Join(", ", changedByUser.ResolverGroupIds
                .Select(catalog.GetResolverGroup)
                .Where(g => g != null)
                .Select(g => g!.Name));
        }

        // El comentario cargado al marcar Resuelto es el borrador interno del grupo
        // resolutor: Mesa de Ayuda puede editarlo antes de enviarlo (ver ConfirmResolution),
        // así que nunca debe mostrársele al cliente, esté confirmado o no. Lo que el
        // cliente sí puede ver es el mensaje final del evento ResolutionConfirmed.
        var isResolutionDraftComment = h.EventType == CaseEventType.StatusChanged && h.Status == CaseStatus.Resuelto;
        var comment = (!canManage && isResolutionDraftComment) ? null : h.Comment;

        return new CaseHistoryRow
        {
            EventType = h.EventType,
            OccurredAtUtc = h.OccurredAtUtc,
            ChangedByName = changedByUser?.Name ?? (h.ChangedByUserId.HasValue ? "(usuario eliminado)" : "Sistema"),
            ChangedByGroupNames = string.IsNullOrWhiteSpace(groupNames) ? null : groupNames,
            Status = h.Status,
            GroupName = h.ResolverGroupId.HasValue ? (catalog.GetResolverGroup(h.ResolverGroupId.Value)?.Name ?? "(grupo eliminado)") : null,
            AssignedUserName = h.AssignedUserId.HasValue ? catalog.GetUser(h.AssignedUserId.Value)?.Name : null,
            CategoryName = h.CategoryId.HasValue ? (catalog.GetCategory(h.CategoryId.Value)?.Name ?? "(categoría eliminada)") : null,
            Comment = comment
        };
    }
}
