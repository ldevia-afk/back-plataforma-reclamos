using System.ComponentModel.DataAnnotations;
using GestionCasos.Web.Models;

namespace GestionCasos.Web.ViewModels;

public class ResolverGroupFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingresá un nombre.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccioná el país.")]
    public int? CountryId { get; set; }

    public List<int> MemberUserIds { get; set; } = new();

    /// <summary>Marca a este grupo como la "Mesa de Ayuda" del país (uno solo por país).</summary>
    public bool IsHelpDesk { get; set; }

    public List<CountryOption> AvailableCountries { get; set; } = new();
    public List<AppUser> AvailableUsers { get; set; } = new();
}

public class CategoryFormViewModel
{
    [Required(ErrorMessage = "Ingresá un nombre.")]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;
}

public class ClientFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingresá un código identificador.")]
    [StringLength(30)]
    public string ExternalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá un nombre.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccioná el país.")]
    public int? CountryId { get; set; }

    public List<CountryOption> AvailableCountries { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();

    [StringLength(120)]
    public string? NewBranchName { get; set; }
}

public class ClientImportResultViewModel
{
    public bool HasRun { get; set; }
    public int ClientsCreated { get; set; }
    public int ClientsUpdated { get; set; }
    public int BranchesCreated { get; set; }
    public int BranchesUpdated { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingresá un nombre.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá un email.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccioná el perfil.")]
    public UserProfileType? ProfileType { get; set; }

    [Required(ErrorMessage = "Seleccioná el país.")]
    public int? CountryId { get; set; }

    // Perfil Cliente
    public List<int> AssignedClientIds { get; set; } = new();

    // Perfil Interno
    public bool HasAllBranches { get; set; }
    public List<int> AssignedBranchIds { get; set; } = new();
    public List<int> ResolverGroupIds { get; set; } = new();

    public List<CountryOption> AvailableCountries { get; set; } = new();
    public List<Client> AvailableClients { get; set; } = new();
    public List<Branch> AvailableBranches { get; set; } = new();
    public List<ResolverGroup> AvailableResolverGroups { get; set; } = new();
}

public class SwitchUserViewModel
{
    public List<AppUser> ClientUsers { get; set; } = new();
    public List<AppUser> InternalUsers { get; set; } = new();
    public Dictionary<int, string> CountryNamesById { get; set; } = new();
}

public class UserDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserProfileType ProfileType { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public List<string> AssignedClientNames { get; set; } = new();
    public bool HasAllBranches { get; set; }
    public List<string> AssignedBranchNames { get; set; } = new();
    public List<string> ResolverGroupNames { get; set; } = new();

    public List<UserHistoryRow> History { get; set; } = new();
}

public class UserHistoryRow
{
    public UserHistoryEventType Type { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string? ResolverGroupName { get; set; }

    public string Description => Type switch
    {
        UserHistoryEventType.Created => "Alta del usuario",
        UserHistoryEventType.Deactivated => "Baja del usuario",
        UserHistoryEventType.Reactivated => "Reactivación del usuario",
        UserHistoryEventType.AddedToGroup => $"Se agregó al grupo resolutor {ResolverGroupName}",
        UserHistoryEventType.RemovedFromGroup => $"Dejó de pertenecer al grupo resolutor {ResolverGroupName}",
        _ => Type.ToString()
    };
}
