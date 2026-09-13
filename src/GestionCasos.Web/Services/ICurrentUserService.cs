using GestionCasos.Web.Models;

namespace GestionCasos.Web.Services;

/// <summary>
/// Punto único de integración con la plataforma existente: hoy devuelve un usuario
/// "simulado" elegido a través de /Session/Switch (no hay login real en este módulo
/// de prueba). El día de mañana, para integrarlo con el login real de la plataforma,
/// basta con reemplazar la implementación de esta interfaz por una que resuelva el
/// usuario a partir de la sesión/claims que ya usa la plataforma.
/// </summary>
public interface ICurrentUserService
{
    AppUser? GetCurrentUser();
    void SetCurrentUser(int userId);
    void ClearCurrentUser();
}
