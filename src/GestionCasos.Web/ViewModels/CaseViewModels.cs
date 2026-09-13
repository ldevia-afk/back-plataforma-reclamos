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
    public string? ResolverGroupName { get; set; }

    public bool CanManage { get; set; }
    public List<ResolverGroupOption> AvailableGroups { get; set; } = new();
    public List<CategoryOption> AvailableCategories { get; set; } = new();

    public List<StatusHistoryRow> StatusHistory { get; set; } = new();
    public List<GroupHistoryRow> GroupHistory { get; set; } = new();
}

public class ResolverGroupOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class StatusHistoryRow
{
    public CaseStatus Status { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string? ChangedByName { get; set; }
}

public class GroupHistoryRow
{
    public string GroupName { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; }
    public string? ChangedByName { get; set; }
    public string? Comment { get; set; }
}

public class InternalInboxViewModel
{
    public List<CaseListItemViewModel> AllCases { get; set; } = new();
    public List<IGrouping<string, CaseListItemViewModel>> GroupedByType { get; set; } = new();

    public List<CategoryOption> Categories { get; set; } = new();
    public List<ResolverGroupOption> ResolverGroups { get; set; } = new();

    public CaseType? FilterType { get; set; }
    public int? FilterCategoryId { get; set; }
    public CaseStatus? FilterStatus { get; set; }
    public int? FilterGroupId { get; set; }
    public bool GroupByType { get; set; }
}
