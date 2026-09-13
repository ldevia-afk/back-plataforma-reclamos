namespace GestionCasos.Web.Models;

/// <summary>
/// Categoría de un caso (Solicitud, Consulta, Reclamo, y las que el perfil interno
/// vaya dando de alta). Las categorías de sistema (seed) no se pueden eliminar.
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSystemDefined { get; set; }
}
