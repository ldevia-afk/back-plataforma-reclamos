using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

public interface ICaseService
{
    /// <summary>Crea un caso nuevo para el usuario cliente indicado.</summary>
    ServiceCase CreateCase(AppUser createdBy, int clientId, List<int> branchIds, int categoryId, string description);

    /// <summary>Casos visibles para un usuario cliente (los de sus clientes asignados).</summary>
    List<ServiceCase> GetCasesForClientUser(AppUser user);

    /// <summary>Casos visibles para un usuario interno (según sus sucursales asignadas / todas).</summary>
    List<ServiceCase> GetCasesForInternalUser(AppUser user);

    ServiceCase? GetCase(int id);

    void ChangeStatus(int caseId, CaseStatus newStatus, int changedByUserId);
    void AssignGroup(int caseId, int? resolverGroupId, int changedByUserId);
}
