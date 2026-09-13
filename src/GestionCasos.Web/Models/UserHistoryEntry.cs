namespace GestionCasos.Web.Models;

public enum UserHistoryEventType
{
    Created = 0,
    Deactivated = 1,
    Reactivated = 2,
    AddedToGroup = 3,
    RemovedFromGroup = 4
}

/// <summary>Un evento del historial de un usuario (alta, baja, alta/baja de un grupo resolutor).</summary>
public class UserHistoryEntry
{
    public UserHistoryEventType Type { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    /// <summary>Grupo resolutor involucrado, sólo para AddedToGroup / RemovedFromGroup.</summary>
    public int? ResolverGroupId { get; set; }
}
