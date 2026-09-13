using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>Panel de métricas: resueltos, pendientes, iniciados, tiempos promedio.</summary>
public class MetricsController : GestionCasosControllerBase
{
    private readonly IMetricsService _metricsService;

    public MetricsController(ICurrentUserService currentUserService, IMetricsService metricsService) : base(currentUserService)
    {
        _metricsService = metricsService;
    }

    public IActionResult Index(int? selectedCountryId, CaseType? selectedType, int? selectedCategoryId)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        return View(_metricsService.BuildMetrics(selectedCountryId, selectedType, selectedCategoryId));
    }
}
