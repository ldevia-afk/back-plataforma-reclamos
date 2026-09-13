namespace GestionCasos.Web.Models;

/// <summary>
/// Grupo resolutor al que el perfil interno puede derivar casos.
/// Está asociado a un país y tiene una lista de usuarios (perfil interno) miembros.
/// </summary>
public class ResolverGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public List<int> MemberUserIds { get; set; } = new();

    /// <summary>
    /// Marca al grupo "Mesa de Ayuda" de ese país: los casos se le derivan
    /// automáticamente al marcarse Resuelto, para que confirme y envíe el
    /// mensaje de resolución al cliente.
    /// </summary>
    public bool IsHelpDesk { get; set; }
}
