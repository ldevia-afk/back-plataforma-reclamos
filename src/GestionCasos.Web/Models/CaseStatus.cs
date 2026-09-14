namespace GestionCasos.Web.Models;

public enum CaseStatus
{
    Inicial = 0,
    Asignado = 1,
    EnAnalisis = 2,
    Resuelto = 3,
    Cerrado = 4
}

public static class CaseStatusExtensions
{
    public static string ToDisplayName(this CaseStatus status) => status switch
    {
        CaseStatus.Inicial => "Inicial",
        CaseStatus.Asignado => "Asignado",
        CaseStatus.EnAnalisis => "Análisis",
        CaseStatus.Resuelto => "Resuelto",
        CaseStatus.Cerrado => "Cerrado",
        _ => status.ToString()
    };
}
