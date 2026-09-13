namespace GestionCasos.Web.Models;

/// <summary>Tipo de caso, elegido por el cliente al registrarlo.</summary>
public enum CaseType
{
    Solicitud = 0,
    Consulta = 1,
    Reclamo = 2
}

public static class CaseTypeExtensions
{
    public static string ToDisplayName(this CaseType type) => type switch
    {
        CaseType.Solicitud => "Solicitud",
        CaseType.Consulta => "Consulta",
        CaseType.Reclamo => "Reclamo",
        _ => type.ToString()
    };
}
