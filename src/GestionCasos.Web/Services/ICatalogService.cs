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
    ResolverGroup CreateResolverGroup(string name, int countryId, List<int> memberUserIds);
    void UpdateResolverGroup(int id, string name, List<int> memberUserIds);
    void DeleteResolverGroup(int id);

    List<AppUser> GetUsers(UserProfileType? profileType = null, int? countryId = null);
    AppUser? GetUser(int id);
    AppUser CreateUser(AppUser user);
    void UpdateUser(AppUser user);
    void DeleteUser(int id);
}
