using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>ABM de usuarios: perfil, país, clientes asignados (Cliente) o sucursales/grupos (Interno).</summary>
public class UsersController : GestionCasosControllerBase
{
    private readonly ICatalogService _catalogService;

    public UsersController(ICurrentUserService currentUserService, ICatalogService catalogService) : base(currentUserService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Index(string? q, int? page, int? pageSize)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var countryScope = ManagementCountryScope;
        var users = _catalogService.GetUsers(includeInactive: true, countryId: countryScope);
        var clients = _catalogService.GetClients();
        var groups = _catalogService.GetResolverGroups();
        var countryNames = _catalogService.GetCountries().ToDictionary(c => c.Id, c => c.Name);

        var items = users.Select(u => new UserListItemViewModel
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            ProfileType = u.ProfileType,
            CountryName = countryNames.TryGetValue(u.CountryId, out var cn) ? cn : string.Empty,
            ScopeDisplay = u.ProfileType == UserProfileType.Cliente
                ? string.Join(", ", u.AssignedClientIds.Select(cid => clients.FirstOrDefault(c => c.Id == cid)?.Name).Where(n => n != null))
                : (u.HasAllBranches ? "Todas las sucursales" : $"{u.AssignedBranchIds.Count} sucursal(es)"),
            GroupNames = string.Join(", ", u.ResolverGroupIds.Select(gid => groups.FirstOrDefault(g => g.Id == gid)?.Name).Where(n => n != null)),
            IsActive = u.IsActive
        }).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            items = items.Where(u =>
                u.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.ProfileDisplay.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.CountryName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.ScopeDisplay.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.GroupNames.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (u.IsActive ? "activo" : "inactivo").Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var vm = new UserListViewModel { Q = q, Paging = items.ToPagedResult(page, pageSize) };
        return View(vm);
    }

    public IActionResult Details(int id)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var user = _catalogService.GetUser(id);
        if (user == null || !InScope(user.CountryId)) return NotFound();

        var vm = new UserDetailViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            ProfileType = user.ProfileType,
            CountryName = _catalogService.GetCountry(user.CountryId)?.Name ?? string.Empty,
            IsActive = user.IsActive,
            AssignedClientNames = user.AssignedClientIds.Select(cid => _catalogService.GetClient(cid)?.Name ?? "(cliente eliminado)").ToList(),
            HasAllBranches = user.HasAllBranches,
            AssignedBranchNames = user.AssignedBranchIds.Select(bid => _catalogService.GetBranch(bid)?.Name ?? "(sucursal eliminada)").ToList(),
            ResolverGroupNames = user.ResolverGroupIds.Select(gid => _catalogService.GetResolverGroup(gid)?.Name ?? "(grupo eliminado)").ToList(),
            History = user.History
                .OrderBy(h => h.OccurredAtUtc)
                .Select(h => new UserHistoryRow
                {
                    Type = h.Type,
                    OccurredAtUtc = h.OccurredAtUtc,
                    ResolverGroupName = h.ResolverGroupId.HasValue ? (_catalogService.GetResolverGroup(h.ResolverGroupId.Value)?.Name ?? "(grupo eliminado)") : null
                })
                .ToList()
        };

        return View(vm);
    }

    public IActionResult Create()
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        return View("Form", BuildFormViewModel(new UserFormViewModel { CountryId = ManagementCountryScope }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserFormViewModel model)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        ApplyScopeAndValidateProfile(model);

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model));
        }

        _catalogService.CreateUser(MapToUser(model));
        TempData["Success"] = "Usuario creado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var user = _catalogService.GetUser(id);
        if (user == null || !InScope(user.CountryId)) return NotFound();

        var model = new UserFormViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            ProfileType = user.ProfileType,
            CountryId = user.CountryId,
            AssignedClientIds = user.AssignedClientIds,
            HasAllBranches = user.HasAllBranches,
            AssignedBranchIds = user.AssignedBranchIds,
            ResolverGroupIds = user.ResolverGroupIds
        };
        return View("Form", BuildFormViewModel(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UserFormViewModel model)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var existing = _catalogService.GetUser(model.Id);
        if (existing == null || !InScope(existing.CountryId)) return NotFound();

        ApplyScopeAndValidateProfile(model);

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model));
        }

        _catalogService.UpdateUser(MapToUser(model));
        TempData["Success"] = "Usuario actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Deactivate(int id)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var user = _catalogService.GetUser(id);
        if (user == null || !InScope(user.CountryId)) return NotFound();

        _catalogService.DeactivateUser(id);
        TempData["Success"] = "Usuario dado de baja. Se desasoció de sus grupos resolutores.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reactivate(int id)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var user = _catalogService.GetUser(id);
        if (user == null || !InScope(user.CountryId)) return NotFound();

        _catalogService.ReactivateUser(id);
        TempData["Success"] = "Usuario reactivado.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>True si el país indicado está dentro del alcance de gestión del usuario actual.</summary>
    private bool InScope(int countryId) => ManagementCountryScope is not { } scope || scope == countryId;

    /// <summary>
    /// Un Administrador de país no puede elegir otro país ni asignar perfiles de administrador
    /// (evita que se auto-otorgue o le otorgue a otro más permisos de los que tiene).
    /// </summary>
    private void ApplyScopeAndValidateProfile(UserFormViewModel model)
    {
        var scope = ManagementCountryScope;
        if (scope.HasValue)
        {
            model.CountryId = scope.Value;
            if (model.ProfileType is UserProfileType.Administrador or UserProfileType.AdministradorPais)
            {
                ModelState.AddModelError(nameof(model.ProfileType), "No tenés permiso para asignar ese perfil.");
            }
        }
    }

    private static AppUser MapToUser(UserFormViewModel model) => new()
    {
        Id = model.Id,
        Name = model.Name.Trim(),
        Email = model.Email.Trim(),
        ProfileType = model.ProfileType!.Value,
        CountryId = model.CountryId!.Value,
        AssignedClientIds = model.ProfileType == UserProfileType.Cliente ? model.AssignedClientIds : new List<int>(),
        HasAllBranches = model.ProfileType == UserProfileType.Interno && model.HasAllBranches,
        AssignedBranchIds = model.ProfileType == UserProfileType.Interno && !model.HasAllBranches ? model.AssignedBranchIds : new List<int>(),
        ResolverGroupIds = model.ProfileType == UserProfileType.Interno ? model.ResolverGroupIds : new List<int>()
    };

    private UserFormViewModel BuildFormViewModel(UserFormViewModel model)
    {
        var countryScope = ManagementCountryScope;
        model.AvailableCountries = _catalogService.GetCountries()
            .Where(c => countryScope == null || c.Id == countryScope.Value)
            .Select(c => new CountryOption { Id = c.Id, Name = c.Name }).ToList();
        model.AvailableClients = _catalogService.GetClients(countryScope);
        model.AvailableBranches = _catalogService.GetBranches(countryId: countryScope);
        model.AvailableResolverGroups = _catalogService.GetResolverGroups(countryScope);
        model.AvailableProfileTypes = countryScope == null
            ? new List<UserProfileType> { UserProfileType.Cliente, UserProfileType.Interno, UserProfileType.Administrador, UserProfileType.AdministradorPais }
            : new List<UserProfileType> { UserProfileType.Cliente, UserProfileType.Interno };
        return model;
    }
}
