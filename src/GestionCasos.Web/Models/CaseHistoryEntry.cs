namespace GestionCasos.Web.Models;

public enum CaseEventType
{
    Created = 0,
    StatusChanged = 1,
    GroupAssigned = 2,
    CategoryAssigned = 3,
    ResolutionConfirmed = 4
}

/// <summary>
/// Un evento del historial unificado de un caso: creación, cambio de estado,
/// derivación a grupo resolutor (con miembro asignado opcional), categorización
/// interna o confirmación de la resolución hacia el cliente.
/// </summary>
public class CaseHistoryEntry
{
    public CaseEventType EventType { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    /// <summary>Null cuando el evento lo generó el sistema (ej. derivación automática a Mesa de Ayuda).</summary>
    public int? ChangedByUserId { get; set; }

    /// <summary>Sólo para StatusChanged / Created: nuevo estado del caso.</summary>
    public CaseStatus? Status { get; set; }

    /// <summary>Sólo para GroupAssigned: grupo resolutor asignado (null = se quitó el grupo).</summary>
    public int? ResolverGroupId { get; set; }
    /// <summary>Sólo para GroupAssigned: miembro específico del grupo asignado al caso (opcional).</summary>
    public int? AssignedUserId { get; set; }

    /// <summary>Sólo para CategoryAssigned: categoría interna asignada (null = se quitó la categoría).</summary>
    public int? CategoryId { get; set; }

    /// <summary>Observación / comentario cargado junto con el evento (opcional).</summary>
    public string? Comment { get; set; }
}
