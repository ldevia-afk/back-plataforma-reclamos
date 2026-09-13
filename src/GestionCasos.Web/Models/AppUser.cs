namespace GestionCasos.Web.Models;

/// <summary>
/// Usuario del módulo. La autenticación real la resuelve la plataforma existente;
/// este módulo sólo necesita saber, para el usuario ya autenticado, su perfil,
/// país y alcance (clientes o sucursales asignadas). Ver
/// <see cref="Services.ICurrentUserService"/> para el punto de integración.
/// </summary>
public class AppUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserProfileType ProfileType { get; set; }
    public int CountryId { get; set; }

    // --- Perfil Cliente ---
    /// <summary>Clientes que este usuario representa (perfil Cliente).</summary>
    public List<int> AssignedClientIds { get; set; } = new();

    // --- Perfil Interno ---
    /// <summary>Si es true, el usuario interno tiene acceso a todas las sucursales del país.</summary>
    public bool HasAllBranches { get; set; }
    /// <summary>Sucursales puntuales asignadas (cuando HasAllBranches es false).</summary>
    public List<int> AssignedBranchIds { get; set; } = new();
    /// <summary>Grupos resolutores de los que este usuario interno es miembro.</summary>
    public List<int> ResolverGroupIds { get; set; } = new();

    /// <summary>Baja lógica: un usuario inactivo no puede operar ni ser elegido en nuevas asignaciones.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Historial de alta, baja y cambios de membresía en grupos resolutores.</summary>
    public List<UserHistoryEntry> History { get; set; } = new();
}
