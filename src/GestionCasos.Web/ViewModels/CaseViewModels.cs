using System.ComponentModel.DataAnnotations;
using GestionCasos.Web.Models;

namespace GestionCasos.Web.ViewModels;

public class ClientWithBranchesOption
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public List<BranchOption> Branches { get; set; } = new();
}

public class BranchOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CaseCreateViewModel
{
    public List<ClientWithBranchesOption> AvailableClients { get; set; } = new();

    [Required(ErrorMessage = "Seleccioná el cliente.")]
    public int? ClientId { get; set; }

    [Required(ErrorMessage = "Seleccioná al menos una sucursal.")]
    [MinLength(1, ErrorMessage = "Seleccioná al menos una sucursal.")]
    public List<int> BranchIds { get; set; } = new();

    [Required(ErrorMessage = "Seleccioná el tipo de caso.")]
    public CaseType? Type { get; set; }

    [Required(ErrorMessage = "Contanos qué necesitás.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "El detalle debe tener entre 5 y 2000 caracteres.")]
    public string Description { get; set; } = string.Empty;
}

public class CaseListItemViewModel
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public CaseType Type { get; set; }
    /// <summary>Categoría interna asignada; null si el perfil interno todavía no la categorizó.</summary>
    public string? CategoryName { get; set; }
    public CaseStatus Status { get; set; }
    public string? ResolverGroupName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool ResolutionConfirmed { get; set; }
    public string? ResolutionComment { get; set; }
    /// <summary>Resuelto pero todavía sin confirmar/enviar al cliente por Mesa de Ayuda (SAC).</summary>
    public bool PendingSacConfirmation => Status == CaseStatus.Resuelto && !ResolutionConfirmed && !string.IsNullOrWhiteSpace(ResolutionComment);
}

public class CaseDetailViewModel
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public CaseType Type { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Description { get; set; } = string.Empty;
    public CaseStatus Status { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public int? ResolverGroupId { get; set; }
    public string? ResolverGroupName { get; set; }
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }

    public bool CanManage { get; set; }
    public List<ResolverGroupOption> AvailableGroups { get; set; } = new();
    public List<CategoryOption> AvailableCategories { get; set; } = new();

    public List<CaseHistoryRow> History { get; set; } = new();

    // --- Resolución y confirmación con el cliente ---
    public DateTime? ResolvedAtUtc { get; set; }
    public string? ResolutionComment { get; set; }
    public bool ResolutionConfirmed { get; set; }
    public DateTime? ResolutionConfirmedAtUtc { get; set; }
    /// <summary>El grupo asignado actualmente es la Mesa de Ayuda del país (puede confirmar la resolución).</summary>
    public bool AssignedToHelpDesk { get; set; }
    /// <summary>El cliente ya puede ver el comentario de resolución.</summary>
    public bool ShowResolutionToClient => Status == CaseStatus.Resuelto && ResolutionConfirmed;
    /// <summary>Resuelto pero todavía sin confirmar/enviar al cliente por Mesa de Ayuda (SAC).</summary>
    public bool PendingSacConfirmation => Status == CaseStatus.Resuelto && !ResolutionConfirmed && !string.IsNullOrWhiteSpace(ResolutionComment);
}

public class ResolverGroupOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<UserOption> Members { get; set; } = new();
}

public class UserOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CaseHistoryRow
{
    public CaseEventType EventType { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string? ChangedByName { get; set; }
    /// <summary>Grupo(s) resolutor(es) al que pertenece el usuario que generó el evento, si aplica.</summary>
    public string? ChangedByGroupNames { get; set; }
    public CaseStatus? Status { get; set; }
    public string? GroupName { get; set; }
    public string? AssignedUserName { get; set; }
    public string? CategoryName { get; set; }
    public string? Comment { get; set; }

    public string Title => EventType switch
    {
        CaseEventType.Created => "Caso creado",
        CaseEventType.StatusChanged => $"Estado cambiado a {Status?.ToDisplayName()}",
        CaseEventType.GroupAssigned => GroupName == null ? "Se quitó el grupo resolutor" : $"Derivado a {GroupName}" + (AssignedUserName != null ? $" (asignado a {AssignedUserName})" : ""),
        CaseEventType.CategoryAssigned => CategoryName == null ? "Se quitó la categoría" : $"Categorizado como \"{CategoryName}\"",
        CaseEventType.ResolutionConfirmed => "Confirmación enviada al cliente",
        _ => EventType.ToString()
    };
}

public class InternalInboxViewModel
{
    /// <summary>Ítems a mostrar: la página actual en la vista lista simple, o todos los que matchean el filtro cuando se agrupa por tipo.</summary>
    public List<CaseListItemViewModel> AllCases { get; set; } = new();
    public List<IGrouping<string, CaseListItemViewModel>> GroupedByType { get; set; } = new();

    /// <summary>Paginado remoto: sólo aplica a la vista "lista simple" (agrupar por tipo muestra el total filtrado como reporte).</summary>
    public PagedResult<CaseListItemViewModel>? Paging { get; set; }

    public List<CategoryOption> Categories { get; set; } = new();
    public List<ResolverGroupOption> ResolverGroups { get; set; } = new();

    public string? Q { get; set; }
    public CaseType? FilterType { get; set; }
    public int? FilterCategoryId { get; set; }
    public CaseStatus? FilterStatus { get; set; }
    public int? FilterGroupId { get; set; }
    /// <summary>Filtra por fecha de registro del caso (CreatedAtUtc), en la zona horaria local.</summary>
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public bool GroupByType { get; set; }
}

public class ClientCaseListViewModel
{
    public PagedResult<CaseListItemViewModel> Paging { get; set; } = new();
    public string? Q { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}
