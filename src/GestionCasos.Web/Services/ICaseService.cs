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

    void ChangeStatus(int caseId, CaseStatus newStatus, int changedByUserId);
    void AssignGroup(int caseId, int? resolverGroupId, int changedByUserId, string? comment);

    /// <summary>Asigna (o quita) la categoría interna de un caso. Sólo la usa el perfil interno.</summary>
    void AssignCategory(int caseId, int? categoryId);
}
