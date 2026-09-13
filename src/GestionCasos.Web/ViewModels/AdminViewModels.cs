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
