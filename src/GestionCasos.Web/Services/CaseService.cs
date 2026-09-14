using GestionCasos.Web.Data;
using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

public class CaseService : ICaseService
{
    private readonly InMemoryDataStore _store;
    private readonly IEmailService _emailService;

    public CaseService(InMemoryDataStore store, IEmailService emailService)
    {
        _store = store;
        _emailService = emailService;
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
                serviceCase.History.Add(new CaseHistoryEntry
                {
                    EventType = CaseEventType.Created,
                    Status = CaseStatus.Inicial,
                    OccurredAtUtc = now,
                    ChangedByUserId = createdBy.Id
                });

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

    public void ChangeStatus(int caseId, CaseStatus newStatus, int changedByUserId, string? resolutionComment = null)
    {
        lock (_store.Lock)
        {
            var serviceCase = _store.Cases.FirstOrDefault(c => c.Id == caseId);
            if (serviceCase == null) return;

            var comment = string.IsNullOrWhiteSpace(resolutionComment) ? null : resolutionComment.Trim();

            serviceCase.Status = newStatus;
            serviceCase.History.Add(new CaseHistoryEntry
            {
                EventType = CaseEventType.StatusChanged,
                Status = newStatus,
                OccurredAtUtc = DateTime.UtcNow,
                ChangedByUserId = changedByUserId,
                Comment = comment
            });

            if (newStatus == CaseStatus.Resuelto)
            {
                serviceCase.ResolutionComment = comment;
                serviceCase.ResolutionConfirmed = false;
                serviceCase.ResolutionConfirmedAtUtc = null;
                serviceCase.ResolutionConfirmedByUserId = null;

                var helpDeskGroup = _store.ResolverGroups.FirstOrDefault(g => g.CountryId == serviceCase.CountryId && g.IsHelpDesk);
                if (helpDeskGroup != null && serviceCase.AssignedGroupId != helpDeskGroup.Id)
                {
                    serviceCase.AssignedGroupId = helpDeskGroup.Id;
                    serviceCase.AssignedUserId = null;
                    serviceCase.History.Add(new CaseHistoryEntry
                    {
                        EventType = CaseEventType.GroupAssigned,
                        ResolverGroupId = helpDeskGroup.Id,
                        OccurredAtUtc = DateTime.UtcNow,
                        ChangedByUserId = null,
                        Comment = "Derivado automáticamente a Mesa de Ayuda para confirmar la resolución con el cliente."
                    });
                }
            }
        }
    }

    public void AssignGroup(int caseId, int? resolverGroupId, int changedByUserId, string? comment, int? assignedUserId = null)
    {
        lock (_store.Lock)
        {
            var serviceCase = _store.Cases.FirstOrDefault(c => c.Id == caseId);
            if (serviceCase == null) return;

            serviceCase.AssignedGroupId = resolverGroupId;
            serviceCase.AssignedUserId = resolverGroupId.HasValue ? assignedUserId : null;

            serviceCase.History.Add(new CaseHistoryEntry
            {
                EventType = CaseEventType.GroupAssigned,
                ResolverGroupId = resolverGroupId,
                AssignedUserId = serviceCase.AssignedUserId,
                OccurredAtUtc = DateTime.UtcNow,
                ChangedByUserId = changedByUserId,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
            });

            if (serviceCase.Status == CaseStatus.Inicial && resolverGroupId.HasValue)
            {
                serviceCase.Status = CaseStatus.Asignado;
                serviceCase.History.Add(new CaseHistoryEntry
                {
                    EventType = CaseEventType.StatusChanged,
                    Status = CaseStatus.Asignado,
                    OccurredAtUtc = DateTime.UtcNow,
                    ChangedByUserId = changedByUserId
                });
            }
        }
    }

    public void AssignCategory(int caseId, int? categoryId, int changedByUserId)
    {
        lock (_store.Lock)
        {
            var serviceCase = _store.Cases.FirstOrDefault(c => c.Id == caseId);
            if (serviceCase == null) return;

            serviceCase.CategoryId = categoryId;
            serviceCase.History.Add(new CaseHistoryEntry
            {
                EventType = CaseEventType.CategoryAssigned,
                CategoryId = categoryId,
                OccurredAtUtc = DateTime.UtcNow,
                ChangedByUserId = changedByUserId
            });
        }
    }

    public string? ManageCase(int caseId, int changedByUserId, int? resolverGroupId, int? assignedUserId, int? categoryId, CaseStatus newStatus, string? comment)
    {
        lock (_store.Lock)
        {
            var serviceCase = _store.Cases.FirstOrDefault(c => c.Id == caseId);
            if (serviceCase == null) return "Caso no encontrado.";

            var trimmedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
            var groupChanged = resolverGroupId != serviceCase.AssignedGroupId || assignedUserId != serviceCase.AssignedUserId;
            var categoryChanged = categoryId != serviceCase.CategoryId;
            var statusChanged = newStatus != serviceCase.Status;

            if (statusChanged && newStatus == CaseStatus.Resuelto && trimmedComment == null)
            {
                return "Para marcar el caso como Resuelto tenés que agregar un comentario con el detalle de la resolución.";
            }

            if (!groupChanged && !categoryChanged && !statusChanged)
            {
                return null;
            }

            // El comentario de observaciones se adjunta al evento principal de la
            // gestión (prioridad: cambio de estado > derivación de grupo > categoría),
            // para no repetirlo en varias entradas del historial a la vez.
            if (groupChanged)
            {
                AssignGroup(caseId, resolverGroupId, changedByUserId, statusChanged ? null : trimmedComment, assignedUserId);
            }
            if (categoryChanged)
            {
                AssignCategory(caseId, categoryId, changedByUserId);
            }
            if (statusChanged)
            {
                ChangeStatus(caseId, newStatus, changedByUserId, trimmedComment);
            }

            return null;
        }
    }

    public void ConfirmResolution(int caseId, string clientMessage, int confirmedByUserId)
    {
        ServiceCase? serviceCase;
        lock (_store.Lock)
        {
            serviceCase = _store.Cases.FirstOrDefault(c => c.Id == caseId);
            if (serviceCase == null || serviceCase.Status != CaseStatus.Resuelto) return;

            serviceCase.ResolutionComment = clientMessage.Trim();
            serviceCase.ResolutionConfirmed = true;
            serviceCase.ResolutionConfirmedAtUtc = DateTime.UtcNow;
            serviceCase.ResolutionConfirmedByUserId = confirmedByUserId;

            serviceCase.History.Add(new CaseHistoryEntry
            {
                EventType = CaseEventType.ResolutionConfirmed,
                OccurredAtUtc = serviceCase.ResolutionConfirmedAtUtc.Value,
                ChangedByUserId = confirmedByUserId,
                Comment = serviceCase.ResolutionComment
            });
        }

        _emailService.SendCaseResolutionEmail(serviceCase, serviceCase.ResolutionComment);
    }
}
