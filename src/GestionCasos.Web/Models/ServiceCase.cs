namespace GestionCasos.Web.Models;

/// <summary>
/// Un caso registrado por un cliente: solicitud de servicio, consulta o reclamo.
/// Aplica a una única sucursal: si el cliente selecciona varias sucursales al
/// mismo tiempo, se genera un caso independiente por cada una (ver CaseService.CreateCase).
/// </summary>
public class ServiceCase
{
    public int Id { get; set; }
    /// <summary>Código visible, ej. "CL-000123".</summary>
    public string Number { get; set; } = string.Empty;

    public int ClientId { get; set; }
    public int BranchId { get; set; }

    /// <summary>Tipo elegido por el cliente al registrar el caso.</summary>
    public CaseType Type { get; set; }

    /// <summary>Categoría interna asignada por el perfil interno (null hasta que la categoricen).</summary>
    public int? CategoryId { get; set; }

    public string Description { get; set; } = string.Empty;

    public int CountryId { get; set; }
    public CaseStatus Status { get; set; } = CaseStatus.Inicial;
    public int? AssignedGroupId { get; set; }

    public int CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public List<CaseStatusHistoryEntry> StatusHistory { get; set; } = new();
    public List<CaseGroupHistoryEntry> GroupHistory { get; set; } = new();

    /// <summary>
    /// Comentario con el detalle de la resolución, cargado por el grupo resolutor
    /// al marcar el caso como Resuelto. No es visible para el cliente hasta que
    /// Mesa de Ayuda lo confirme (ver ResolutionConfirmed).
    /// </summary>
    public string? ResolutionComment { get; set; }

    /// <summary>True una vez que Mesa de Ayuda confirmó y "envió" el mensaje al cliente.</summary>
    public bool ResolutionConfirmed { get; set; }
    public DateTime? ResolutionConfirmedAtUtc { get; set; }
    public int? ResolutionConfirmedByUserId { get; set; }
}
