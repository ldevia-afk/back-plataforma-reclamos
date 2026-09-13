namespace GestionCasos.Web.Models;

public class Branch
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ClientId { get; set; }

    /// <summary>Denormalizado desde el cliente para facilitar filtros por país.</summary>
    public int CountryId { get; set; }
}
