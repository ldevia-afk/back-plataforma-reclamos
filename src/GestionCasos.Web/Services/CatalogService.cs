using GestionCasos.Web.Data;
using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

public class CatalogService : ICatalogService
{
    private readonly InMemoryDataStore _store;

    public CatalogService(InMemoryDataStore store)
    {
        _store = store;
    }

    public List<Country> GetCountries()
    {
        lock (_store.Lock) return _store.Countries.OrderBy(c => c.Name).ToList();
    }

    public Country? GetCountry(int id)
    {
        lock (_store.Lock) return _store.Countries.FirstOrDefault(c => c.Id == id);
    }

    public List<Client> GetClients(int? countryId = null)
    {
        lock (_store.Lock)
        {
            var query = _store.Clients.AsEnumerable();
            if (countryId.HasValue) query = query.Where(c => c.CountryId == countryId.Value);
            return query.OrderBy(c => c.Name).ToList();
        }
    }

    public Client? GetClient(int id)
    {
        lock (_store.Lock) return _store.Clients.FirstOrDefault(c => c.Id == id);
    }

    public Client CreateClient(string name, int countryId)
    {
        lock (_store.Lock)
        {
            var client = new Client { Id = _store.NextClientId(), Name = name, CountryId = countryId };
            _store.Clients.Add(client);
            return client;
        }
    }

    public void UpdateClient(int id, string name, int countryId)
    {
        lock (_store.Lock)
        {
            var client = _store.Clients.FirstOrDefault(c => c.Id == id);
            if (client == null) return;
            client.Name = name;
            client.CountryId = countryId;
            foreach (var branch in _store.Branches.Where(b => b.ClientId == id))
            {
                branch.CountryId = countryId;
            }
        }
    }

    public void DeleteClient(int id)
    {
        lock (_store.Lock)
        {
            _store.Clients.RemoveAll(c => c.Id == id);
            _store.Branches.RemoveAll(b => b.ClientId == id);
            foreach (var user in _store.Users)
            {
                user.AssignedClientIds.Remove(id);
            }
        }
    }

    public List<Branch> GetBranches(int? clientId = null, int? countryId = null)
    {
        lock (_store.Lock)
        {
            var query = _store.Branches.AsEnumerable();
            if (clientId.HasValue) query = query.Where(b => b.ClientId == clientId.Value);
            if (countryId.HasValue) query = query.Where(b => b.CountryId == countryId.Value);
            return query.OrderBy(b => b.Name).ToList();
        }
    }

    public Branch? GetBranch(int id)
    {
        lock (_store.Lock) return _store.Branches.FirstOrDefault(b => b.Id == id);
    }

    public Branch CreateBranch(string name, int clientId)
    {
        lock (_store.Lock)
        {
            var client = _store.Clients.First(c => c.Id == clientId);
            var branch = new Branch { Id = _store.NextBranchId(), Name = name, ClientId = clientId, CountryId = client.CountryId };
            _store.Branches.Add(branch);
            return branch;
        }
    }

    public void UpdateBranch(int id, string name)
    {
        lock (_store.Lock)
        {
            var branch = _store.Branches.FirstOrDefault(b => b.Id == id);
            if (branch == null) return;
            branch.Name = name;
        }
    }

    public void DeleteBranch(int id)
    {
        lock (_store.Lock)
        {
            _store.Branches.RemoveAll(b => b.Id == id);
            foreach (var user in _store.Users)
            {
                user.AssignedBranchIds.Remove(id);
            }
            foreach (var c in _store.Cases)
            {
                c.BranchIds.Remove(id);
            }
        }
    }

    public List<Category> GetCategories()
    {
        lock (_store.Lock) return _store.Categories.OrderBy(c => c.IsSystemDefined ? 0 : 1).ThenBy(c => c.Name).ToList();
    }

    public Category? GetCategory(int id)
    {
        lock (_store.Lock) return _store.Categories.FirstOrDefault(c => c.Id == id);
    }

    public Category CreateCategory(string name)
    {
        lock (_store.Lock)
        {
            var category = new Category { Id = _store.NextCategoryId(), Name = name, IsSystemDefined = false };
            _store.Categories.Add(category);
            return category;
        }
    }

    public void DeleteCategory(int id)
    {
        lock (_store.Lock)
        {
            var category = _store.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null || category.IsSystemDefined) return;
            _store.Categories.Remove(category);
        }
    }

    public List<ResolverGroup> GetResolverGroups(int? countryId = null)
    {
        lock (_store.Lock)
        {
            var query = _store.ResolverGroups.AsEnumerable();
            if (countryId.HasValue) query = query.Where(g => g.CountryId == countryId.Value);
            return query.OrderBy(g => g.Name).ToList();
        }
    }

    public ResolverGroup? GetResolverGroup(int id)
    {
        lock (_store.Lock) return _store.ResolverGroups.FirstOrDefault(g => g.Id == id);
    }

    public ResolverGroup CreateResolverGroup(string name, int countryId, List<int> memberUserIds)
    {
        lock (_store.Lock)
        {
            var group = new ResolverGroup { Id = _store.NextResolverGroupId(), Name = name, CountryId = countryId, MemberUserIds = memberUserIds };
            _store.ResolverGroups.Add(group);
            foreach (var userId in memberUserIds)
            {
                var user = _store.Users.FirstOrDefault(u => u.Id == userId);
                user?.ResolverGroupIds.Add(group.Id);
            }
            return group;
        }
    }

    public void UpdateResolverGroup(int id, string name, List<int> memberUserIds)
    {
        lock (_store.Lock)
        {
            var group = _store.ResolverGroups.FirstOrDefault(g => g.Id == id);
            if (group == null) return;
            group.Name = name;

            foreach (var userId in group.MemberUserIds.Except(memberUserIds).ToList())
            {
                _store.Users.FirstOrDefault(u => u.Id == userId)?.ResolverGroupIds.Remove(id);
            }
            foreach (var userId in memberUserIds.Except(group.MemberUserIds).ToList())
            {
                var user = _store.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null && !user.ResolverGroupIds.Contains(id)) user.ResolverGroupIds.Add(id);
            }

            group.MemberUserIds = memberUserIds;
        }
    }

    public void DeleteResolverGroup(int id)
    {
        lock (_store.Lock)
        {
            _store.ResolverGroups.RemoveAll(g => g.Id == id);
            foreach (var user in _store.Users)
            {
                user.ResolverGroupIds.Remove(id);
            }
            foreach (var c in _store.Cases.Where(c => c.AssignedGroupId == id))
            {
                c.AssignedGroupId = null;
            }
        }
    }

    public List<AppUser> GetUsers(UserProfileType? profileType = null, int? countryId = null)
    {
        lock (_store.Lock)
        {
            var query = _store.Users.AsEnumerable();
            if (profileType.HasValue) query = query.Where(u => u.ProfileType == profileType.Value);
            if (countryId.HasValue) query = query.Where(u => u.CountryId == countryId.Value);
            return query.OrderBy(u => u.Name).ToList();
        }
    }

    public AppUser? GetUser(int id)
    {
        lock (_store.Lock) return _store.Users.FirstOrDefault(u => u.Id == id);
    }

    public AppUser CreateUser(AppUser user)
    {
        lock (_store.Lock)
        {
            user.Id = _store.NextUserId();
            _store.Users.Add(user);
            foreach (var groupId in user.ResolverGroupIds)
            {
                var group = _store.ResolverGroups.FirstOrDefault(g => g.Id == groupId);
                if (group != null && !group.MemberUserIds.Contains(user.Id)) group.MemberUserIds.Add(user.Id);
            }
            return user;
        }
    }

    public void UpdateUser(AppUser user)
    {
        lock (_store.Lock)
        {
            var existing = _store.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existing == null) return;

            foreach (var groupId in existing.ResolverGroupIds.Except(user.ResolverGroupIds).ToList())
            {
                _store.ResolverGroups.FirstOrDefault(g => g.Id == groupId)?.MemberUserIds.Remove(user.Id);
            }
            foreach (var groupId in user.ResolverGroupIds.Except(existing.ResolverGroupIds).ToList())
            {
                var group = _store.ResolverGroups.FirstOrDefault(g => g.Id == groupId);
                if (group != null && !group.MemberUserIds.Contains(user.Id)) group.MemberUserIds.Add(user.Id);
            }

            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.ProfileType = user.ProfileType;
            existing.CountryId = user.CountryId;
            existing.AssignedClientIds = user.AssignedClientIds;
            existing.HasAllBranches = user.HasAllBranches;
            existing.AssignedBranchIds = user.AssignedBranchIds;
            existing.ResolverGroupIds = user.ResolverGroupIds;
        }
    }

    public void DeleteUser(int id)
    {
        lock (_store.Lock)
        {
            _store.Users.RemoveAll(u => u.Id == id);
            foreach (var group in _store.ResolverGroups)
            {
                group.MemberUserIds.Remove(id);
            }
        }
    }
}
