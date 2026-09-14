using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

/// <summary>
/// Envío de emails al cliente. Este módulo de prueba no tiene un proveedor de
/// correo real configurado: en vez de despachar el mensaje, EmailService lo
/// guarda como SentEmail para poder verlo desde la app. Conectar un proveedor
/// real (SMTP, SendGrid, etc.) es el único cambio necesario en la implementación.
/// </summary>
public interface IEmailService
{
    SentEmail SendCaseResolutionEmail(ServiceCase serviceCase, string resolutionMessage);
    List<SentEmail> GetEmailsForCase(int caseId);
    /// <summary>Todos los emails "enviados" (más nuevo primero), para poder revisarlos como prueba.</summary>
    List<SentEmail> GetAllEmails();
}
