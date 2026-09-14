namespace GestionCasos.Web.Models;

/// <summary>
/// Categoría interna para clasificar un caso ya recibido (ej. "Sin movimiento
/// causa cliente", "No se enviaron los insumos"). La da de alta y la asigna
/// únicamente el perfil interno; no la elige el cliente al crear el caso.
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
}
