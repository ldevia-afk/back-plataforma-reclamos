namespace GestionCasos.Web.Models;

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }

    /// <summary>
    /// Código identificador del cliente definido por el negocio (único). Sirve para
    /// asociar sucursales al importar desde Excel sin depender del Id interno.
    /// </summary>
    public string ExternalCode { get; set; } = string.Empty;
}
