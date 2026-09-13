using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

public interface ICaseService
{
    /// <summary>
    /// Crea un caso por cada sucursal indicada (si el cliente selecciona varias
    /// sucursales a la vez, se generan casos independientes, uno por sucursal).
    /// </summary>
    List<ServiceCase> CreateCases(AppUser createdBy, int clientId, List<int> branchIds, CaseType caseType, string description);

    /// <summary>Casos visibles para un usuario cliente (los de sus clientes asignados).</summary>
    List<ServiceCase> GetCasesForClientUser(AppUser user);

    /// <summary>Casos visibles para un usuario interno (según sus sucursales asignadas / todas).</summary>
    List<ServiceCase> GetCasesForInternalUser(AppUser user);

    ServiceCase? GetCase(int id);

    /// <summary>
    /// Cambia el estado del caso. Al pasar a Resuelto es obligatorio indicar
    /// <paramref name="resolutionComment"/>: el caso se deriva automáticamente al
    /// grupo "Mesa de Ayuda" del país para que confirme el mensaje antes de que
    /// llegue al cliente (ver ConfirmResolution).
    /// </summary>
    void ChangeStatus(int caseId, CaseStatus newStatus, int changedByUserId, string? resolutionComment = null);

    void AssignGroup(int caseId, int? resolverGroupId, int changedByUserId, string? comment);

    /// <summary>Asigna (o quita) la categoría interna de un caso. Sólo la usa el perfil interno.</summary>
    void AssignCategory(int caseId, int? categoryId);

    /// <summary>
    /// Mesa de Ayuda confirma (y opcionalmente edita) el mensaje de resolución y
    /// dispara el envío del email al cliente con el detalle del caso.
    /// </summary>
    void ConfirmResolution(int caseId, string clientMessage, int confirmedByUserId);
}
