namespace GestionCasos.Web.ViewModels;

/// <summary>Vista no genérica de un PagedResult&lt;T&gt;, para que la partial de paginado no dependa de T.</summary>
public interface IPagedResult
{
    int Page { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
}

/// <summary>
/// Una página de resultados ya recortada en el servidor (paginación remota): la vista
/// sólo recibe los ítems de la página actual, nunca la lista completa.
/// </summary>
public class PagedResult<T> : IPagedResult
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = Paging.DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public static class Paging
{
    public const int DefaultPageSize = 10;
    public static readonly int[] AllowedPageSizes = { 5, 10, 25, 50 };

    public static int NormalizePageSize(int? requested)
    {
        if (requested.HasValue && AllowedPageSizes.Contains(requested.Value)) return requested.Value;
        return DefaultPageSize;
    }

    /// <summary>
    /// Recorta <paramref name="source"/> a la página pedida. <paramref name="source"/> ya
    /// debería venir filtrado y ordenado; acá sólo se aplica Skip/Take (el equivalente,
    /// contra un backend real, de pedir una página puntual en vez de traer todo).
    /// </summary>
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int? page, int? pageSize)
    {
        var size = NormalizePageSize(pageSize);
        var list = source as IReadOnlyCollection<T> ?? source.ToList();
        var totalCount = list.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)size));
        var current = page.HasValue && page.Value > 0 ? Math.Min(page.Value, totalPages) : 1;

        return new PagedResult<T>
        {
            Items = list.Skip((current - 1) * size).Take(size).ToList(),
            Page = current,
            PageSize = size,
            TotalCount = totalCount
        };
    }
}
