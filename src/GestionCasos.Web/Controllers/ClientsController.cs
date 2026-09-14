using ClosedXML.Excel;
using GestionCasos.Web.Models;
using GestionCasos.Web.Services;
using GestionCasos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionCasos.Web.Controllers;

/// <summary>ABM de clientes y sus sucursales/puntos (perfil Interno).</summary>
public class ClientsController : GestionCasosControllerBase
{
    private readonly ICatalogService _catalogService;

    public ClientsController(ICurrentUserService currentUserService, ICatalogService catalogService) : base(currentUserService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Index()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        var clients = _catalogService.GetClients();
        ViewBag.CountryNames = _catalogService.GetCountries().ToDictionary(c => c.Id, c => c.Name);
        ViewBag.BranchCounts = clients.ToDictionary(c => c.Id, c => _catalogService.GetBranches(clientId: c.Id).Count);
        return View(clients);
    }

    public IActionResult Create()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        return View("Form", BuildFormViewModel(new ClientFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ClientFormViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

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
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        var client = _catalogService.GetClient(id);
        if (client == null) return NotFound();

        var model = new ClientFormViewModel { Id = client.Id, Name = client.Name, CountryId = client.CountryId, ExternalCode = client.ExternalCode };
        return View("Form", BuildFormViewModel(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ClientFormViewModel model)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

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
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _catalogService.DeleteClient(id);
        TempData["Success"] = "Cliente eliminado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddBranch(int clientId, string newBranchName, string? newBranchAddress)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

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
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _catalogService.DeactivateBranch(branchId);
        TempData["Success"] = "Sucursal dada de baja.";
        return RedirectToAction(nameof(Edit), new { id = clientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReactivateBranch(int branchId, int clientId)
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        _catalogService.ReactivateBranch(branchId);
        TempData["Success"] = "Sucursal reactivada.";
        return RedirectToAction(nameof(Edit), new { id = clientId });
    }

    private ClientFormViewModel BuildFormViewModel(ClientFormViewModel model)
    {
        model.AvailableCountries = _catalogService.GetCountries().Select(c => new CountryOption { Id = c.Id, Name = c.Name }).ToList();
        if (model.Id != 0)
        {
            model.Branches = _catalogService.GetBranches(clientId: model.Id, includeInactive: true);
        }
        return model;
    }

    [HttpGet]
    public IActionResult Import()
    {
        var guard = RequireProfile(UserProfileType.Interno);
        if (guard != null) return guard;

        return View(new ClientImportResultViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public IActionResult Import(IFormFile? file)
    {
        var guard = RequireProfile(UserProfileType.Interno);
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

            var existing = _catalogService.GetClientByExternalCode(code);
            if (existing != null)
            {
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
        var guard = RequireProfile(UserProfileType.Interno);
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
