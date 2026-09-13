using GestionCasos.Web.Data;
using GestionCasos.Web.Models;
using Microsoft.AspNetCore.Http;

namespace GestionCasos.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private const string CookieName = "gc_current_user_id";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly InMemoryDataStore _store;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, InMemoryDataStore store)
    {
        _httpContextAccessor = httpContextAccessor;
        _store = store;
    }

    public AppUser? GetCurrentUser()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        if (!context.Request.Cookies.TryGetValue(CookieName, out var raw) || !int.TryParse(raw, out var userId))
        {
            return null;
        }

        lock (_store.Lock)
        {
            return _store.Users.FirstOrDefault(u => u.Id == userId);
        }
    }

    public void SetCurrentUser(int userId)
    {
        var context = _httpContextAccessor.HttpContext;
        context?.Response.Cookies.Append(CookieName, userId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public void ClearCurrentUser()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookieName);
    }
}
