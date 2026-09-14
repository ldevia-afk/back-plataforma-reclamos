namespace GestionCasos.Web.ViewModels;

public class SentEmailListItemViewModel
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}

public class SentEmailListViewModel
{
    public PagedResult<SentEmailListItemViewModel> Paging { get; set; } = new();
    public string? Q { get; set; }
}
