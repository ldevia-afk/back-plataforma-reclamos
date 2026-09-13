using GestionCasos.Web.ViewModels;

namespace GestionCasos.Web.Services;

public interface IMetricsService
{
    MetricsViewModel BuildMetrics(int? countryId, int? categoryId);
}
