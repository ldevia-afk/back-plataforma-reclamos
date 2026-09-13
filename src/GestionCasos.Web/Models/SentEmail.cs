namespace GestionCasos.Web.Models;

/// <summary>
/// Registro de un email "enviado" al cliente. Este módulo de prueba no tiene
/// configurado un servidor de correo real: en vez de despachar el mensaje,
/// IEmailService lo guarda acá para poder verlo en el detalle del caso.
/// Conectar un proveedor de email real es el único cambio necesario para
/// que esto se convierta en un envío efectivo.
/// </summary>
public class SentEmail
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}
