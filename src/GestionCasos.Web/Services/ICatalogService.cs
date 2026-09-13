using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

public interface ICatalogService
{
    List<Country> GetCountries();
    Country? GetCountry(int id);

    List<Client> GetClients(int? countryId = null);
    Client? GetClient(int id);
    Client CreateClient(string name, int countryId);
    void UpdateClient(int id, string name, int countryId);
    void DeleteClient(int id);

    List<Branch> GetBranches(int? clientId = null, int? countryId = null);
    Branch? GetBranch(int id);
    Branch CreateBranch(string name, int clientId);
    void UpdateBranch(int id, string name);
    void DeleteBranch(int id);

    List<Category> GetCategories();
    Category? GetCategory(int id);
    Category CreateCategory(string name);
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
