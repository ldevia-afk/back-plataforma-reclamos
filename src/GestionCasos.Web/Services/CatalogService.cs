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

    public Client? GetClientByExternalCode(string externalCode)
    {
        lock (_store.Lock)
        {
            return _store.Clients.FirstOrDefault(c => c.ExternalCode.Equals(externalCode, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool IsExternalCodeAvailable(string externalCode, int? excludeClientId = null)
    {
        lock (_store.Lock)
        {
            return !_store.Clients.Any(c =>
                c.Id != excludeClientId &&
                c.ExternalCode.Equals(externalCode, StringComparison.OrdinalIgnoreCase));
        }
    }

    public Client CreateClient(string name, int countryId, string externalCode)
    {
        lock (_store.Lock)
        {
            var client = new Client { Id = _store.NextClientId(), Name = name, CountryId = countryId, ExternalCode = externalCode };
            _store.Clients.Add(client);
            return client;
        }
    }

    public void UpdateClient(int id, string name, int countryId, string externalCode)
    {
        lock (_store.Lock)
        {
            var client = _store.Clients.FirstOrDefault(c => c.Id == id);
            if (client == null) return;
            client.Name = name;
            client.CountryId = countryId;
            client.ExternalCode = externalCode;
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

    public List<Branch> GetBranches(int? clientId = null, int? countryId = null, bool includeInactive = false)
    {
        lock (_store.Lock)
        {
            var query = _store.Branches.AsEnumerable();
            if (!includeInactive) query = query.Where(b => b.IsActive);
            if (clientId.HasValue) query = query.Where(b => b.ClientId == clientId.Value);
            if (countryId.HasValue) query = query.Where(b => b.CountryId == countryId.Value);
            return query.OrderBy(b => b.Name).ToList();
        }
    }

    public Branch? GetBranch(int id)
    {
        lock (_store.Lock) return _store.Branches.FirstOrDefault(b => b.Id == id);
    }

    public Branch CreateBranch(string name, int clientId, string? address = null)
    {
        lock (_store.Lock)
        {
            var client = _store.Clients.First(c => c.Id == clientId);
            var branch = new Branch
            {
                Id = _store.NextBranchId(),
                Name = name,
                Address = address?.Trim() ?? string.Empty,
                ClientId = clientId,
                CountryId = client.CountryId
            };
            _store.Branches.Add(branch);
            return branch;
        }
    }

    public void UpdateBranch(int id, string name, string? address)
    {
        lock (_store.Lock)
        {
            var branch = _store.Branches.FirstOrDefault(b => b.Id == id);
            if (branch == null) return;
            branch.Name = name;
            branch.Address = address?.Trim() ?? string.Empty;
        }
    }

    public void DeactivateBranch(int id)
    {
        lock (_store.Lock)
        {
            var branch = _store.Branches.FirstOrDefault(b => b.Id == id);
            if (branch == null) return;
            branch.IsActive = false;
        }
    }

    public void ReactivateBranch(int id)
    {
        lock (_store.Lock)
        {
            var branch = _store.Branches.FirstOrDefault(b => b.Id == id);
            if (branch == null) return;
            branch.IsActive = true;
        }
    }

    public List<Category> GetCategories(int? countryId = null)
    {
        lock (_store.Lock)
        {
            var query = _store.Categories.AsEnumerable();
            if (countryId.HasValue) query = query.Where(c => c.CountryId == countryId.Value);
            return query.OrderBy(c => c.Name).ToList();
        }
    }

    public Category? GetCategory(int id)
    {
        lock (_store.Lock) return _store.Categories.FirstOrDefault(c => c.Id == id);
    }

    public Category CreateCategory(string name, int countryId)
    {
        lock (_store.Lock)
        {
            var category = new Category { Id = _store.NextCategoryId(), Name = name, CountryId = countryId };
            _store.Categories.Add(category);
            return category;
        }
    }

    public void DeleteCategory(int id)
    {
        lock (_store.Lock)
        {
            _store.Categories.RemoveAll(c => c.Id == id);
            foreach (var c in _store.Cases.Where(c => c.CategoryId == id))
            {
                c.CategoryId = null;
            }
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

    public ResolverGroup CreateResolverGroup(string name, int countryId, List<int> memberUserIds, bool isHelpDesk = false)
    {
        lock (_store.Lock)
        {
            var group = new ResolverGroup { Id = _store.NextResolverGroupId(), Name = name, CountryId = countryId, MemberUserIds = memberUserIds, IsHelpDesk = isHelpDesk };
            _store.ResolverGroups.Add(group);
            foreach (var userId in memberUserIds)
            {
                var user = _store.Users.FirstOrDefault(u => u.Id == userId);
                user?.ResolverGroupIds.Add(group.Id);
            }
            if (isHelpDesk) UnsetOtherHelpDeskGroups(countryId, group.Id);
            return group;
        }
    }

    public void UpdateResolverGroup(int id, string name, List<int> memberUserIds, bool isHelpDesk)
    {
        lock (_store.Lock)
        {
            var group = _store.ResolverGroups.FirstOrDefault(g => g.Id == id);
            if (group == null) return;
            group.Name = name;
            group.IsHelpDesk = isHelpDesk;
            if (isHelpDesk) UnsetOtherHelpDeskGroups(group.CountryId, group.Id);

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

    /// <summary>Sólo puede haber un grupo Mesa de Ayuda (IsHelpDesk) por país.</summary>
    private void UnsetOtherHelpDeskGroups(int countryId, int exceptGroupId)
    {
        foreach (var other in _store.ResolverGroups.Where(g => g.CountryId == countryId && g.Id != exceptGroupId && g.IsHelpDesk))
        {
            other.IsHelpDesk = false;
        }
    }

    public List<AppUser> GetUsers(UserProfileType? profileType = null, int? countryId = null, bool includeInactive = false)
    {
        lock (_store.Lock)
        {
            var query = _store.Users.AsEnumerable();
            if (!includeInactive) query = query.Where(u => u.IsActive);
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
            user.IsActive = true;
            var now = DateTime.UtcNow;
            user.History.Add(new UserHistoryEntry { Type = UserHistoryEventType.Created, OccurredAtUtc = now });

            _store.Users.Add(user);
            foreach (var groupId in user.ResolverGroupIds)
            {
                var group = _store.ResolverGroups.FirstOrDefault(g => g.Id == groupId);
                if (group != null && !group.MemberUserIds.Contains(user.Id)) group.MemberUserIds.Add(user.Id);
                user.History.Add(new UserHistoryEntry { Type = UserHistoryEventType.AddedToGroup, OccurredAtUtc = now, ResolverGroupId = groupId });
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

            var now = DateTime.UtcNow;
            foreach (var groupId in existing.ResolverGroupIds.Except(user.ResolverGroupIds).ToList())
            {
                _store.ResolverGroups.FirstOrDefault(g => g.Id == groupId)?.MemberUserIds.Remove(user.Id);
                existing.History.Add(new UserHistoryEntry { Type = UserHistoryEventType.RemovedFromGroup, OccurredAtUtc = now, ResolverGroupId = groupId });
            }
            foreach (var groupId in user.ResolverGroupIds.Except(existing.ResolverGroupIds).ToList())
            {
                var group = _store.ResolverGroups.FirstOrDefault(g => g.Id == groupId);
                if (group != null && !group.MemberUserIds.Contains(user.Id)) group.MemberUserIds.Add(user.Id);
                existing.History.Add(new UserHistoryEntry { Type = UserHistoryEventType.AddedToGroup, OccurredAtUtc = now, ResolverGroupId = groupId });
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

    public void DeactivateUser(int id)
    {
        lock (_store.Lock)
        {
            var user = _store.Users.FirstOrDefault(u => u.Id == id);
            if (user == null || !user.IsActive) return;

            var now = DateTime.UtcNow;
            foreach (var groupId in user.ResolverGroupIds.ToList())
            {
                _store.ResolverGroups.FirstOrDefault(g => g.Id == groupId)?.MemberUserIds.Remove(id);
                user.History.Add(new UserHistoryEntry { Type = UserHistoryEventType.RemovedFromGroup, OccurredAtUtc = now, ResolverGroupId = groupId });
            }
            user.ResolverGroupIds = new List<int>();

            user.IsActive = false;
            user.History.Add(new UserHistoryEntry { Type = UserHistoryEventType.Deactivated, OccurredAtUtc = now });
        }
    }

    public void ReactivateUser(int id)
    {
        lock (_store.Lock)
        {
            var user = _store.Users.FirstOrDefault(u => u.Id == id);
            if (user == null || user.IsActive) return;

            user.IsActive = true;
            user.History.Add(new UserHistoryEntry { Type = UserHistoryEventType.Reactivated, OccurredAtUtc = DateTime.UtcNow });
        }
    }
}
