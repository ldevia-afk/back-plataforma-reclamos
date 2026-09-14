using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>
/// Bandeja de emails "enviados" (sólo perfil Interno): este módulo de prueba no tiene
/// un servidor de correo real conectado, así que acá se puede revisar el contenido de
/// cada email simulado para probar el flujo de confirmación de resolución.
/// </summary>
public class EmailsController : GestionCasosControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ICaseService _caseService;
    private readonly ICatalogService _catalogService;

    public EmailsController(ICurrentUserService currentUserService, IEmailService emailService, ICaseService caseService, ICatalogService catalogService)
        : base(currentUserService)
    {
        _emailService = emailService;
        _caseService = caseService;
        _catalogService = catalogService;
    }

    public IActionResult Index(string? q, int? page, int? pageSize)
    {
        var guard = RequireAnyProfile(UserProfileType.Interno, UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var user = CurrentUser!;
        // Sólo se listan emails de casos dentro del alcance del usuario (mismo país / sucursales asignadas).
        var visibleCases = _caseService.GetCasesForInternalUser(user).ToDictionary(c => c.Id);

        var items = _emailService.GetAllEmails()
            .Where(e => visibleCases.ContainsKey(e.CaseId))
            .Select(e => new SentEmailListItemViewModel
            {
                Id = e.Id,
                CaseId = e.CaseId,
                CaseNumber = visibleCases[e.CaseId].Number,
                ToEmail = e.ToEmail,
                Subject = e.Subject,
                Body = e.Body,
                SentAtUtc = e.SentAtUtc
            })
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            items = items.Where(e =>
                e.CaseNumber.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.ToEmail.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.Subject.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var vm = new SentEmailListViewModel
        {
            Q = q,
            Paging = items.ToPagedResult(page, pageSize)
        };
        return View(vm);
    }
}
