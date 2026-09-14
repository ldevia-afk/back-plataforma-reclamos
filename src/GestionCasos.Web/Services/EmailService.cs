using GestionCasos.Web.Data;
using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

public class EmailService : IEmailService
{
    private readonly InMemoryDataStore _store;
    private readonly ICatalogService _catalogService;

    public EmailService(InMemoryDataStore store, ICatalogService catalogService)
    {
        _store = store;
        _catalogService = catalogService;
    }

    public SentEmail SendCaseResolutionEmail(ServiceCase serviceCase, string resolutionMessage)
    {
        var branch = _catalogService.GetBranch(serviceCase.BranchId);
        var creator = _catalogService.GetUser(serviceCase.CreatedByUserId);
        var toEmail = creator?.Email ?? string.Empty;

        var startDate = serviceCase.CreatedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        var endDate = (serviceCase.History.LastOrDefault(h => h.Status == CaseStatus.Resuelto)?.OccurredAtUtc ?? DateTime.UtcNow)
            .ToLocalTime().ToString("dd/MM/yyyy HH:mm");

        var body =
            $"Caso: {serviceCase.Number}\n" +
            $"Sucursal: {branch?.Name ?? "(sucursal eliminada)"}\n" +
            $"Fecha de inicio: {startDate}\n" +
            $"Fecha de finalización: {endDate}\n" +
            $"Estado final: {serviceCase.Status.ToDisplayName()}\n\n" +
            $"Comentario de resolución:\n{resolutionMessage}";

        lock (_store.Lock)
        {
            var email = new SentEmail
            {
                Id = _store.NextSentEmailId(),
                CaseId = serviceCase.Id,
                ToEmail = toEmail,
                Subject = $"Resolución del caso {serviceCase.Number}",
                Body = body,
                SentAtUtc = DateTime.UtcNow
            };
            _store.SentEmails.Add(email);
            return email;
        }
    }

    public List<SentEmail> GetEmailsForCase(int caseId)
    {
        lock (_store.Lock)
        {
            return _store.SentEmails.Where(e => e.CaseId == caseId).OrderBy(e => e.SentAtUtc).ToList();
        }
    }
}
