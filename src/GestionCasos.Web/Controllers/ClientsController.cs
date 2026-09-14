using ClosedXML.Excel;
using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>ABM de clientes y sus sucursales/puntos (perfiles Administrador / Administrador de país).</summary>
public class ClientsController : GestionCasosControllerBase
{
    private readonly ICatalogService _catalogService;

    public ClientsController(ICurrentUserService currentUserService, ICatalogService catalogService) : base(currentUserService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Index(string? q, int? filterCountryId, int? page, int? pageSize)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var countryScope = ManagementCountryScope;
        // Administrador de país: el filtro de país queda fijo en el suyo, sin importar lo que llegue por query string.
        var effectiveCountryId = countryScope ?? filterCountryId;

        var countryNames = _catalogService.GetCountries().ToDictionary(c => c.Id, c => c.Name);
        var items = _catalogService.GetClients(effectiveCountryId)
            .Select(c => new ClientListItemViewModel
            {
                Id = c.Id,
                ExternalCode = c.ExternalCode,
                Name = c.Name,
                CountryName = countryNames.TryGetValue(c.CountryId, out var cn) ? cn : string.Empty,
                BranchCount = _catalogService.GetBranches(clientId: c.Id, includeInactive: true).Count
            })
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            items = items.Where(c =>
                c.ExternalCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.CountryName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var vm = new ClientListViewModel
        {
            Q = q,
            FilterCountryId = effectiveCountryId,
            CanChooseCountry = countryScope == null,
            AvailableCountries = _catalogService.GetCountries().Select(c => new CountryOption { Id = c.Id, Name = c.Name }).ToList(),
            Paging = items.ToPagedResult(page, pageSize)
        };
        return View(vm);
    }

    public IActionResult Branches(string? q, int? filterCountryId, int? filterClientId, bool? filterIsActive, int? page, int? pageSize)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var countryScope = ManagementCountryScope;
        var effectiveCountryId = countryScope ?? filterCountryId;

        var clientsById = _catalogService.GetClients().ToDictionary(c => c.Id);
        var countryNames = _catalogService.GetCountries().ToDictionary(c => c.Id, c => c.Name);

        var items = _catalogService.GetBranches(clientId: filterClientId, countryId: effectiveCountryId, includeInactive: true)
            .Select(b =>
            {
                clientsById.TryGetValue(b.ClientId, out var client);
                return new BranchListItemViewModel
                {
                    Id = b.Id,
                    ClientId = b.ClientId,
                    ClientName = client?.Name ?? "(cliente eliminado)",
                    ClientCode = client?.ExternalCode ?? string.Empty,
                    CountryName = countryNames.TryGetValue(b.CountryId, out var cn) ? cn : string.Empty,
                    Name = b.Name,
                    Address = b.Address,
                    IsActive = b.IsActive
                };
            })
            .OrderBy(b => b.ClientName).ThenBy(b => b.Name)
            .AsEnumerable();

        if (filterIsActive.HasValue)
        {
            items = items.Where(b => b.IsActive == filterIsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            items = items.Where(b =>
                b.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.ClientName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.ClientCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.CountryName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.Address.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (b.IsActive ? "activa" : "inactiva").Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var vm = new BranchListViewModel
        {
            Q = q,
            FilterCountryId = effectiveCountryId,
            FilterClientId = filterClientId,
            FilterIsActive = filterIsActive,
            CanChooseCountry = countryScope == null,
            AvailableCountries = _catalogService.GetCountries().Select(c => new CountryOption { Id = c.Id, Name = c.Name }).ToList(),
            AvailableClients = _catalogService.GetClients(countryScope),
            Paging = items.ToPagedResult(page, pageSize)
        };
        return View(vm);
    }

    public IActionResult Create()
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        return View("Form", BuildFormViewModel(new ClientFormViewModel { CountryId = ManagementCountryScope }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ClientFormViewModel model)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        // Administrador de país: el país queda fijo en el suyo, sin importar lo que llegue del formulario.
        model.CountryId = ManagementCountryScope ?? model.CountryId;

        if (model.ExternalCode != null && !_catalogService.IsExternalCodeAvailable(model.ExternalCode.Trim()))
        {
            ModelState.AddModelError(nameof(model.ExternalCode), "Ya existe un cliente con ese código.");
        }

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model));
        }

        var client = _catalogService.CreateClient(model.Name.Trim(), model.CountryId!.Value, model.ExternalCode?.Trim() ?? string.Empty);
        TempData["Success"] = "Cliente creado. Ahora podés agregarle sucursales.";
        return RedirectToAction(nameof(Edit), new { id = client.Id });
    }

    public IActionResult Edit(int id)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var client = _catalogService.GetClient(id);
        if (client == null || !InScope(client.CountryId)) return NotFound();

        var model = new ClientFormViewModel { Id = client.Id, Name = client.Name, CountryId = client.CountryId, ExternalCode = client.ExternalCode };
        return View("Form", BuildFormViewModel(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ClientFormViewModel model)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var existing = _catalogService.GetClient(model.Id);
        if (existing == null || !InScope(existing.CountryId)) return NotFound();

        // Administrador de país: el país queda fijo en el suyo, sin importar lo que llegue del formulario.
        model.CountryId = ManagementCountryScope ?? model.CountryId;

        if (model.ExternalCode != null && !_catalogService.IsExternalCodeAvailable(model.ExternalCode.Trim(), model.Id))
        {
            ModelState.AddModelError(nameof(model.ExternalCode), "Ya existe un cliente con ese código.");
        }

        if (!ModelState.IsValid)
        {
            return View("Form", BuildFormViewModel(model));
        }

        _catalogService.UpdateClient(model.Id, model.Name.Trim(), model.CountryId!.Value, model.ExternalCode?.Trim() ?? string.Empty);
        TempData["Success"] = "Cliente actualizado.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var client = _catalogService.GetClient(id);
        if (client == null || !InScope(client.CountryId))
        {
            TempData["Error"] = "No tenés acceso a ese cliente.";
            return RedirectToAction(nameof(Index));
        }

        _catalogService.DeleteClient(id);
        TempData["Success"] = "Cliente eliminado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddBranch(int clientId, string newBranchName, string? newBranchAddress)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var client = _catalogService.GetClient(clientId);
        if (client == null || !InScope(client.CountryId)) return NotFound();

        if (!string.IsNullOrWhiteSpace(newBranchName))
        {
            _catalogService.CreateBranch(newBranchName.Trim(), clientId, newBranchAddress);
            TempData["Success"] = "Sucursal agregada.";
        }
        return RedirectToAction(nameof(Edit), new { id = clientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeactivateBranch(int branchId, int clientId)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var client = _catalogService.GetClient(clientId);
        if (client == null || !InScope(client.CountryId)) return NotFound();

        _catalogService.DeactivateBranch(branchId);
        TempData["Success"] = "Sucursal dada de baja.";
        return RedirectToAction(nameof(Edit), new { id = clientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReactivateBranch(int branchId, int clientId)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var client = _catalogService.GetClient(clientId);
        if (client == null || !InScope(client.CountryId)) return NotFound();

        _catalogService.ReactivateBranch(branchId);
        TempData["Success"] = "Sucursal reactivada.";
        return RedirectToAction(nameof(Edit), new { id = clientId });
    }

    private ClientFormViewModel BuildFormViewModel(ClientFormViewModel model)
    {
        var countryScope = ManagementCountryScope;
        model.AvailableCountries = _catalogService.GetCountries()
            .Where(c => countryScope == null || c.Id == countryScope.Value)
            .Select(c => new CountryOption { Id = c.Id, Name = c.Name }).ToList();
        if (model.Id != 0)
        {
            model.Branches = _catalogService.GetBranches(clientId: model.Id, includeInactive: true);
        }
        return model;
    }

    /// <summary>True si el país indicado está dentro del alcance de gestión del usuario actual.</summary>
    private bool InScope(int countryId) => ManagementCountryScope is not { } scope || scope == countryId;

    [HttpGet]
    public IActionResult Import()
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        return View(new ClientImportResultViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public IActionResult Import(IFormFile? file)
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        var result = new ClientImportResultViewModel { HasRun = true };

        if (file == null || file.Length == 0)
        {
            result.Errors.Add("Seleccioná un archivo .xlsx para importar.");
            return View(result);
        }

        XLWorkbook workbook;
        try
        {
            using var stream = file.OpenReadStream();
            workbook = new XLWorkbook(stream);
        }
        catch (Exception)
        {
            result.Errors.Add("No se pudo leer el archivo. Verificá que sea un Excel (.xlsx) válido.");
            return View(result);
        }

        using (workbook)
        {
            var clientesSheet = workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("Clientes", StringComparison.OrdinalIgnoreCase));
            var sucursalesSheet = workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("Sucursales", StringComparison.OrdinalIgnoreCase));

            if (clientesSheet == null && sucursalesSheet == null)
            {
                result.Errors.Add("El archivo debe tener una hoja \"Clientes\" y/o una hoja \"Sucursales\". Descargá la plantilla para ver el formato esperado.");
                return View(result);
            }

            if (clientesSheet != null)
            {
                ImportClientes(clientesSheet, result);
            }

            if (sucursalesSheet != null)
            {
                ImportSucursales(sucursalesSheet, result);
            }
        }

        TempData["Success"] = "Importación finalizada.";
        return View(result);
    }

    private void ImportClientes(IXLWorksheet sheet, ClientImportResultViewModel result)
    {
        var countries = _catalogService.GetCountries();
        var countryScope = ManagementCountryScope;

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var code = row.Cell(1).GetString().Trim();
            var name = row.Cell(2).GetString().Trim();
            var countryText = row.Cell(3).GetString().Trim();

            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name)) continue; // fila en blanco

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                result.Errors.Add($"Clientes, fila {row.RowNumber()}: faltan datos (código y nombre son obligatorios).");
                continue;
            }

            var country = countries.FirstOrDefault(c =>
                c.Name.Equals(countryText, StringComparison.OrdinalIgnoreCase) ||
                c.Code.Equals(countryText, StringComparison.OrdinalIgnoreCase));
            if (country == null)
            {
                result.Errors.Add($"Clientes, fila {row.RowNumber()}: país \"{countryText}\" no reconocido (usá Chile/Argentina o CL/AR).");
                continue;
            }

            if (countryScope.HasValue && country.Id != countryScope.Value)
            {
                result.Errors.Add($"Clientes, fila {row.RowNumber()}: no tenés permiso para dar de alta clientes en \"{countryText}\".");
                continue;
            }

            var existing = _catalogService.GetClientByExternalCode(code);
            if (existing != null)
            {
                if (countryScope.HasValue && existing.CountryId != countryScope.Value)
                {
                    result.Errors.Add($"Clientes, fila {row.RowNumber()}: no tenés permiso para modificar el cliente \"{code}\".");
                    continue;
                }
                _catalogService.UpdateClient(existing.Id, name, country.Id, code);
                result.ClientsUpdated++;
            }
            else
            {
                _catalogService.CreateClient(name, country.Id, code);
                result.ClientsCreated++;
            }
        }
    }

    private void ImportSucursales(IXLWorksheet sheet, ClientImportResultViewModel result)
    {
        var countryScope = ManagementCountryScope;

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var clientCode = row.Cell(1).GetString().Trim();
            var name = row.Cell(2).GetString().Trim();
            var address = row.Cell(3).GetString().Trim();

            if (string.IsNullOrWhiteSpace(clientCode) && string.IsNullOrWhiteSpace(name)) continue; // fila en blanco

            if (string.IsNullOrWhiteSpace(clientCode) || string.IsNullOrWhiteSpace(name))
            {
                result.Errors.Add($"Sucursales, fila {row.RowNumber()}: faltan datos (código de cliente y nombre son obligatorios).");
                continue;
            }

            var client = _catalogService.GetClientByExternalCode(clientCode);
            if (client == null)
            {
                result.Errors.Add($"Sucursales, fila {row.RowNumber()}: no existe ningún cliente con código \"{clientCode}\".");
                continue;
            }

            if (countryScope.HasValue && client.CountryId != countryScope.Value)
            {
                result.Errors.Add($"Sucursales, fila {row.RowNumber()}: no tenés permiso para modificar sucursales del cliente \"{clientCode}\".");
                continue;
            }

            var existingBranch = _catalogService.GetBranches(clientId: client.Id, includeInactive: true)
                .FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existingBranch != null)
            {
                _catalogService.UpdateBranch(existingBranch.Id, name, address);
                result.BranchesUpdated++;
            }
            else
            {
                _catalogService.CreateBranch(name, client.Id, address);
                result.BranchesCreated++;
            }
        }
    }

    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        var guard = RequireAnyProfile(UserProfileType.Administrador, UserProfileType.AdministradorPais);
        if (guard != null) return guard;

        using var workbook = new XLWorkbook();

        var wsClientes = workbook.Worksheets.Add("Clientes");
        wsClientes.Cell(1, 1).Value = "Código";
        wsClientes.Cell(1, 2).Value = "Nombre";
        wsClientes.Cell(1, 3).Value = "País";
        wsClientes.Cell(2, 1).Value = "1001";
        wsClientes.Cell(2, 2).Value = "Supermercados Líder";
        wsClientes.Cell(2, 3).Value = "Chile";
        wsClientes.Range("A1:C1").Style.Font.Bold = true;
        wsClientes.Columns().AdjustToContents();

        var wsSucursales = workbook.Worksheets.Add("Sucursales");
        wsSucursales.Cell(1, 1).Value = "Código Cliente";
        wsSucursales.Cell(1, 2).Value = "Nombre";
        wsSucursales.Cell(1, 3).Value = "Dirección";
        wsSucursales.Cell(2, 1).Value = "1001";
        wsSucursales.Cell(2, 2).Value = "Líder Providencia";
        wsSucursales.Cell(2, 3).Value = "Av. Providencia 2124, Santiago";
        wsSucursales.Range("A1:C1").Style.Font.Bold = true;
        wsSucursales.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "plantilla-clientes-sucursales.xlsx");
    }
}
