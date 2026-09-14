using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

public interface ICatalogService
{
    List<Country> GetCountries();
    Country? GetCountry(int id);

    List<Client> GetClients(int? countryId = null);
    Client? GetClient(int id);
    Client? GetClientByExternalCode(string externalCode);
    /// <summary>True si nadie más (salvo <paramref name="excludeClientId"/>) usa ese código.</summary>
    bool IsExternalCodeAvailable(string externalCode, int? excludeClientId = null);
    Client CreateClient(string name, int countryId, string externalCode);
    void UpdateClient(int id, string name, int countryId, string externalCode);
    void DeleteClient(int id);

    /// <summary>Por defecto sólo devuelve sucursales activas; <paramref name="includeInactive"/> trae también las dadas de baja.</summary>
    List<Branch> GetBranches(int? clientId = null, int? countryId = null, bool includeInactive = false);
    Branch? GetBranch(int id);
    Branch CreateBranch(string name, int clientId, string? address = null);
    void UpdateBranch(int id, string name, string? address);

    /// <summary>Baja lógica: una sucursal inactiva deja de poder elegirse para casos nuevos.</summary>
    void DeactivateBranch(int id);
    void ReactivateBranch(int id);

    List<Category> GetCategories(int? countryId = null);
    Category? GetCategory(int id);
    Category CreateCategory(string name, int countryId);
    void DeleteCategory(int id);

    List<ResolverGroup> GetResolverGroups(int? countryId = null);
    ResolverGroup? GetResolverGroup(int id);
    ResolverGroup CreateResolverGroup(string name, int countryId, List<int> memberUserIds, bool isHelpDesk = false);
    void UpdateResolverGroup(int id, string name, List<int> memberUserIds, bool isHelpDesk);
    void DeleteResolverGroup(int id);

    /// <summary>Por defecto sólo devuelve usuarios activos; <paramref name="includeInactive"/> trae también los dados de baja.</summary>
    List<AppUser> GetUsers(UserProfileType? profileType = null, int? countryId = null, bool includeInactive = false);
    AppUser? GetUser(int id);
    AppUser CreateUser(AppUser user);
    void UpdateUser(AppUser user);

    /// <summary>Baja lógica: desactiva al usuario y lo desasocia de todos sus grupos resolutores.</summary>
    void DeactivateUser(int id);
    void ReactivateUser(int id);
}
