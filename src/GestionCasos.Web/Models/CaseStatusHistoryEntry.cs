namespace GestionCasos.Web.Models;

public class CaseStatusHistoryEntry
{
    public CaseStatus Status { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public int? ChangedByUserId { get; set; }
}
