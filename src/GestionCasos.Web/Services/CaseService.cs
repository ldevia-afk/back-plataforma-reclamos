using GestionCasos.Web.Data;
using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

public class CaseService : ICaseService
{
    private readonly InMemoryDataStore _store;

    public CaseService(InMemoryDataStore store)
    {
        _store = store;
    }

    public List<ServiceCase> CreateCases(AppUser createdBy, int clientId, List<int> branchIds, CaseType caseType, string description)
    {
        lock (_store.Lock)
        {
            var client = _store.Clients.First(c => c.Id == clientId);
            var countryCode = _store.Countries.First(c => c.Id == client.CountryId).Code;
            var created = new List<ServiceCase>();

            foreach (var branchId in branchIds)
            {
                var now = DateTime.UtcNow;
                var id = _store.NextCaseId();

                var serviceCase = new ServiceCase
                {
                    Id = id,
                    Number = $"{countryCode}-{id:D6}",
                    ClientId = clientId,
                    BranchId = branchId,
                    Type = caseType,
                    CategoryId = null,
                    Description = description,
                    CountryId = client.CountryId,
                    Status = CaseStatus.Inicial,
                    CreatedByUserId = createdBy.Id,
                    CreatedAtUtc = now
                };
                serviceCase.StatusHistory.Add(new CaseStatusHistoryEntry { Status = CaseStatus.Inicial, ChangedAtUtc = now, ChangedByUserId = createdBy.Id });
                serviceCase.GroupHistory.Add(new CaseGroupHistoryEntry { ResolverGroupId = null, ChangedAtUtc = now, ChangedByUserId = createdBy.Id });

                _store.Cases.Add(serviceCase);
                created.Add(serviceCase);
            }

            return created;
        }
    }

    public List<ServiceCase> GetCasesForClientUser(AppUser user)
    {
        lock (_store.Lock)
        {
            return _store.Cases
                .Where(c => user.AssignedClientIds.Contains(c.ClientId))
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToList();
        }
    }

    public List<ServiceCase> GetCasesForInternalUser(AppUser user)
    {
        lock (_store.Lock)
        {
            var query = _store.Cases.Where(c => c.CountryId == user.CountryId);
            if (!user.HasAllBranches)
            {
                query = query.Where(c => user.AssignedBranchIds.Contains(c.BranchId));
            }
            return query.OrderByDescending(c => c.CreatedAtUtc).ToList();
        }
    }

    public ServiceCase? GetCase(int id)
    {
        lock (_store.Lock) return _store.Cases.FirstOrDefault(c => c.Id == id);
    }

    public void ChangeStatus(int caseId, CaseStatus newStatus, int changedByUserId)
    {
        lock (_store.Lock)
        {
            var serviceCase = _store.Cases.FirstOrDefault(c => c.Id == caseId);
            if (serviceCase == null) return;

            serviceCase.Status = newStatus;
            serviceCase.StatusHistory.Add(new CaseStatusHistoryEntry
            {
                Status = newStatus,
                ChangedAtUtc = DateTime.UtcNow,
                ChangedByUserId = changedByUserId
            });
        }
    }

    public void AssignGroup(int caseId, int? resolverGroupId, int changedByUserId, string? comment)
    {
        lock (_store.Lock)
        {
            var serviceCase = _store.Cases.FirstOrDefault(c => c.Id == caseId);
            if (serviceCase == null) return;

            serviceCase.AssignedGroupId = resolverGroupId;
            serviceCase.GroupHistory.Add(new CaseGroupHistoryEntry
            {
                ResolverGroupId = resolverGroupId,
                ChangedAtUtc = DateTime.UtcNow,
                ChangedByUserId = changedByUserId,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
            });

            if (serviceCase.Status == CaseStatus.Inicial && resolverGroupId.HasValue)
            {
                serviceCase.Status = CaseStatus.Asignado;
                serviceCase.StatusHistory.Add(new CaseStatusHistoryEntry
                {
                    Status = CaseStatus.Asignado,
                    ChangedAtUtc = DateTime.UtcNow,
                    ChangedByUserId = changedByUserId
                });
            }
        }
    }

    public void AssignCategory(int caseId, int? categoryId)
    {
        lock (_store.Lock)
        {
            var serviceCase = _store.Cases.FirstOrDefault(c => c.Id == caseId);
            if (serviceCase == null) return;

            serviceCase.CategoryId = categoryId;
        }
    }
}
