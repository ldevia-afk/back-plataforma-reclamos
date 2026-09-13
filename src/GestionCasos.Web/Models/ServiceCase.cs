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
    public int CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;

    public int CountryId { get; set; }
    public CaseStatus Status { get; set; } = CaseStatus.Inicial;
    public int? AssignedGroupId { get; set; }

    public int CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public List<CaseStatusHistoryEntry> StatusHistory { get; set; } = new();
    public List<CaseGroupHistoryEntry> GroupHistory { get; set; } = new();
}
