using GestionCasos.Web.Models;

namespace GestionCasos.Web.ViewModels;

public class MetricsViewModel
{
    public int TotalCases { get; set; }
    public int InitiatedCount { get; set; }
    public int ResolvedCount { get; set; }
    public int PendingCount { get; set; }

    public double? AvgResolutionHours { get; set; }
    public double? AvgHandoffHours { get; set; }

    public List<StatusCountItem> CountsByStatus { get; set; } = new();
    public List<TypeCountItem> CountsByType { get; set; } = new();
    public List<CategoryCountItem> CountsByCategory { get; set; } = new();
    public List<StatusDurationItem> AvgHoursByStatus { get; set; } = new();
    public List<GroupTransitionItem> GroupTransitions { get; set; } = new();

    public List<CountryOption> Countries { get; set; } = new();
    public List<CategoryOption> Categories { get; set; } = new();
    public int? SelectedCountryId { get; set; }
    public CaseType? SelectedType { get; set; }
    public int? SelectedCategoryId { get; set; }
}

public class TypeCountItem
{
    public CaseType Type { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class StatusCountItem
{
    public CaseStatus Status { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class CategoryCountItem
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class StatusDurationItem
{
    public CaseStatus Status { get; set; }
    public double AvgHours { get; set; }
    public int SampleCount { get; set; }
}

public class GroupTransitionItem
{
    public string FromGroupName { get; set; } = string.Empty;
    public string ToGroupName { get; set; } = string.Empty;
    public double AvgHours { get; set; }
    public int Count { get; set; }
}

public class CountryOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CategoryOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
