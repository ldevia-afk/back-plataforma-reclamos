namespace GestionCasos.Web.Models;

public enum CaseStatus
{
    Inicial = 0,
    Asignado = 1,
    EnAnalisis = 2,
    Resuelto = 3
}

public static class CaseStatusExtensions
{
    public static string ToDisplayName(this CaseStatus status) => status switch
    {
        CaseStatus.Inicial => "Inicial",
        CaseStatus.Asignado => "Asignado",
        CaseStatus.EnAnalisis => "En análisis",
        CaseStatus.Resuelto => "Resuelto",
        _ => status.ToString()
    };
}
