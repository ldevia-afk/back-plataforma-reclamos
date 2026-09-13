namespace GestionCasos.Web.Models;

public class CaseGroupHistoryEntry
{
    /// <summary>Null = el caso quedó sin grupo asignado (bandeja general).</summary>
    public int? ResolverGroupId { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public int? ChangedByUserId { get; set; }
    /// <summary>Comentario u observación opcional cargada al derivar el caso.</summary>
    public string? Comment { get; set; }
}
