using GestionCasos.Web.Data;
using GestionCasos.Web.Models;
using GestionCasos.Web.ViewModels;

namespace GestionCasos.Web.Services;

public class MetricsService : IMetricsService
{
    private readonly InMemoryDataStore _store;

    public MetricsService(InMemoryDataStore store)
    {
        _store = store;
    }

    public MetricsViewModel BuildMetrics(int? countryId, int? categoryId)
    {
        lock (_store.Lock)
        {
            var cases = _store.Cases.AsEnumerable();
            if (countryId.HasValue) cases = cases.Where(c => c.CountryId == countryId.Value);
            if (categoryId.HasValue) cases = cases.Where(c => c.CategoryId == categoryId.Value);
            var caseList = cases.ToList();

            var vm = new MetricsViewModel
            {
                TotalCases = caseList.Count,
                InitiatedCount = caseList.Count,
                ResolvedCount = caseList.Count(c => c.Status == CaseStatus.Resuelto),
                PendingCount = caseList.Count(c => c.Status != CaseStatus.Resuelto),
                Countries = _store.Countries.Select(c => new CountryOption { Id = c.Id, Name = c.Name }).OrderBy(c => c.Name).ToList(),
                Categories = _store.Categories.Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).OrderBy(c => c.Name).ToList(),
                SelectedCountryId = countryId,
                SelectedCategoryId = categoryId
            };

            // Promedio de resolución: desde creación hasta que entra en estado Resuelto.
            var resolutionDurations = new List<double>();
            foreach (var c in caseList)
            {
                var resolvedEntry = c.StatusHistory.Where(h => h.Status == CaseStatus.Resuelto).OrderBy(h => h.ChangedAtUtc).FirstOrDefault();
                if (resolvedEntry != null)
                {
                    resolutionDurations.Add((resolvedEntry.ChangedAtUtc - c.CreatedAtUtc).TotalHours);
                }
            }
            vm.AvgResolutionHours = resolutionDurations.Count > 0 ? resolutionDurations.Average() : null;

            // Conteo y % por estado.
            var total = caseList.Count == 0 ? 1 : caseList.Count;
            vm.CountsByStatus = Enum.GetValues<CaseStatus>()
                .Select(s => new StatusCountItem
                {
                    Status = s,
                    Count = caseList.Count(c => c.Status == s),
                    Percentage = Math.Round(caseList.Count(c => c.Status == s) * 100.0 / total, 1)
                })
                .ToList();

            // Conteo y % por categoría.
            vm.CountsByCategory = caseList
                .GroupBy(c => c.CategoryId)
                .Select(g => new CategoryCountItem
                {
                    CategoryName = _store.Categories.FirstOrDefault(cat => cat.Id == g.Key)?.Name ?? "(sin categoría)",
                    Count = g.Count(),
                    Percentage = Math.Round(g.Count() * 100.0 / total, 1)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Tiempo promedio de permanencia en cada estado antes de pasar al siguiente.
            var durationsByStatus = new Dictionary<CaseStatus, List<double>>();
            foreach (var c in caseList)
            {
                var ordered = c.StatusHistory.OrderBy(h => h.ChangedAtUtc).ToList();
                for (var i = 0; i < ordered.Count - 1; i++)
                {
                    var hours = (ordered[i + 1].ChangedAtUtc - ordered[i].ChangedAtUtc).TotalHours;
                    if (!durationsByStatus.TryGetValue(ordered[i].Status, out var list))
                    {
                        list = new List<double>();
                        durationsByStatus[ordered[i].Status] = list;
                    }
                    list.Add(hours);
                }
            }
            vm.AvgHoursByStatus = durationsByStatus
                .Select(kvp => new StatusDurationItem { Status = kvp.Key, AvgHours = Math.Round(kvp.Value.Average(), 1), SampleCount = kvp.Value.Count })
                .OrderBy(x => x.Status)
                .ToList();

            // Tiempo promedio de un grupo resolutor a otro (handoff) + detalle por par de grupos.
            var handoffDurations = new List<double>();
            var transitions = new Dictionary<(int? from, int? to), List<double>>();
            foreach (var c in caseList)
            {
                var ordered = c.GroupHistory.OrderBy(h => h.ChangedAtUtc).ToList();
                for (var i = 0; i < ordered.Count - 1; i++)
                {
                    if (!ordered[i].ResolverGroupId.HasValue) continue; // sólo cuenta como "handoff" si venía de un grupo asignado
                    var hours = (ordered[i + 1].ChangedAtUtc - ordered[i].ChangedAtUtc).TotalHours;
                    handoffDurations.Add(hours);

                    var key = (ordered[i].ResolverGroupId, ordered[i + 1].ResolverGroupId);
                    if (!transitions.TryGetValue(key, out var list))
                    {
                        list = new List<double>();
                        transitions[key] = list;
                    }
                    list.Add(hours);
                }
            }
            vm.AvgHandoffHours = handoffDurations.Count > 0 ? handoffDurations.Average() : null;

            vm.GroupTransitions = transitions
                .Select(kvp => new GroupTransitionItem
                {
                    FromGroupName = GroupName(kvp.Key.from),
                    ToGroupName = GroupName(kvp.Key.to),
                    AvgHours = Math.Round(kvp.Value.Average(), 1),
                    Count = kvp.Value.Count
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return vm;
        }
    }

    private string GroupName(int? groupId)
    {
        if (!groupId.HasValue) return "Sin grupo";
        return _store.ResolverGroups.FirstOrDefault(g => g.Id == groupId.Value)?.Name ?? "(grupo eliminado)";
    }
}
