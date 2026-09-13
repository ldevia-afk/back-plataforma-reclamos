using GestionCasos.Web.Models;

namespace GestionCasos.Web.Data;

/// <summary>
/// Almacén de datos en memoria para este módulo de prueba (sin base de datos real).
/// Registrado como singleton: todos los servicios comparten estas mismas listas.
/// Reemplazar por un acceso a base de datos real es el único cambio necesario
/// para pasar a producción; el resto del código depende sólo de los modelos.
/// </summary>
public class InMemoryDataStore
{
    private readonly object _lock = new();

    public List<Country> Countries { get; } = new();
    public List<Client> Clients { get; } = new();
    public List<Branch> Branches { get; } = new();
    public List<Category> Categories { get; } = new();
    public List<ResolverGroup> ResolverGroups { get; } = new();
    public List<AppUser> Users { get; } = new();
    public List<ServiceCase> Cases { get; } = new();

    private int _nextClientId = 1;
    private int _nextBranchId = 1;
    private int _nextCategoryId = 1;
    private int _nextResolverGroupId = 1;
    private int _nextUserId = 1;
    private int _nextCaseId = 1;

    public object Lock => _lock;

    public int NextClientId() { lock (_lock) return _nextClientId++; }
    public int NextBranchId() { lock (_lock) return _nextBranchId++; }
    public int NextCategoryId() { lock (_lock) return _nextCategoryId++; }
    public int NextResolverGroupId() { lock (_lock) return _nextResolverGroupId++; }
    public int NextUserId() { lock (_lock) return _nextUserId++; }
    public int NextCaseId() { lock (_lock) return _nextCaseId++; }

    public InMemoryDataStore()
    {
        Seed();
    }

    private void Seed()
    {
        // --- Países ---
        const int chileId = 1;
        const int argentinaId = 2;
        Countries.Add(new Country { Id = chileId, Name = "Chile", Code = "CL" });
        Countries.Add(new Country { Id = argentinaId, Name = "Argentina", Code = "AR" });

        // --- Categorías internas (las asigna el perfil interno luego de recibir el caso) ---
        var catSinMovimientoCliente = new Category { Id = _nextCategoryId++, Name = "Sin movimiento causa cliente" };
        var catInsumosNoEnviados = new Category { Id = _nextCategoryId++, Name = "No se enviaron los insumos" };
        var catFallaTecnica = new Category { Id = _nextCategoryId++, Name = "Falla técnica del equipo" };
        Categories.AddRange(new[] { catSinMovimientoCliente, catInsumosNoEnviados, catFallaTecnica });

        // --- Clientes ---
        var lider = new Client { Id = _nextClientId++, Name = "Supermercados Líder", CountryId = chileId };
        var bancoChile = new Client { Id = _nextClientId++, Name = "Banco de Chile", CountryId = chileId };
        var farmacity = new Client { Id = _nextClientId++, Name = "Farmacity", CountryId = argentinaId };
        var bancoGalicia = new Client { Id = _nextClientId++, Name = "Banco Galicia", CountryId = argentinaId };
        Clients.AddRange(new[] { lider, bancoChile, farmacity, bancoGalicia });

        // --- Sucursales ---
        var brLiderProvidencia = new Branch { Id = _nextBranchId++, Name = "Líder Providencia", ClientId = lider.Id, CountryId = chileId };
        var brLiderLasCondes = new Branch { Id = _nextBranchId++, Name = "Líder Las Condes", ClientId = lider.Id, CountryId = chileId };
        var brLiderMaipu = new Branch { Id = _nextBranchId++, Name = "Líder Maipú", ClientId = lider.Id, CountryId = chileId };
        var brBancoChileCentro = new Branch { Id = _nextBranchId++, Name = "Sucursal Centro", ClientId = bancoChile.Id, CountryId = chileId };
        var brBancoChileNunoa = new Branch { Id = _nextBranchId++, Name = "Sucursal Ñuñoa", ClientId = bancoChile.Id, CountryId = chileId };
        var brFarmacityPalermo = new Branch { Id = _nextBranchId++, Name = "Farmacity Palermo", ClientId = farmacity.Id, CountryId = argentinaId };
        var brFarmacityRecoleta = new Branch { Id = _nextBranchId++, Name = "Farmacity Recoleta", ClientId = farmacity.Id, CountryId = argentinaId };
        var brGaliciaMicrocentro = new Branch { Id = _nextBranchId++, Name = "Sucursal Microcentro", ClientId = bancoGalicia.Id, CountryId = argentinaId };
        var brGaliciaBelgrano = new Branch { Id = _nextBranchId++, Name = "Sucursal Belgrano", ClientId = bancoGalicia.Id, CountryId = argentinaId };
        Branches.AddRange(new[]
        {
            brLiderProvidencia, brLiderLasCondes, brLiderMaipu,
            brBancoChileCentro, brBancoChileNunoa,
            brFarmacityPalermo, brFarmacityRecoleta,
            brGaliciaMicrocentro, brGaliciaBelgrano
        });

        // --- Grupos resolutores ---
        var mesaAyudaCl = new ResolverGroup { Id = _nextResolverGroupId++, Name = "Mesa de Ayuda CL", CountryId = chileId };
        var logisticaCl = new ResolverGroup { Id = _nextResolverGroupId++, Name = "Logística CL", CountryId = chileId };
        var mesaAyudaAr = new ResolverGroup { Id = _nextResolverGroupId++, Name = "Mesa de Ayuda AR", CountryId = argentinaId };
        var seguridadAr = new ResolverGroup { Id = _nextResolverGroupId++, Name = "Seguridad AR", CountryId = argentinaId };
        ResolverGroups.AddRange(new[] { mesaAyudaCl, logisticaCl, mesaAyudaAr, seguridadAr });

        // --- Usuarios ---
        var clienteCl1 = new AppUser
        {
            Id = _nextUserId++, Name = "María Fernández", Email = "maria.fernandez@lider.cl",
            ProfileType = UserProfileType.Cliente, CountryId = chileId,
            AssignedClientIds = new List<int> { lider.Id }
        };
        var clienteCl2 = new AppUser
        {
            Id = _nextUserId++, Name = "Jorge Soto", Email = "jorge.soto@bancochile.cl",
            ProfileType = UserProfileType.Cliente, CountryId = chileId,
            AssignedClientIds = new List<int> { bancoChile.Id }
        };
        var clienteAr1 = new AppUser
        {
            Id = _nextUserId++, Name = "Lucía Gómez", Email = "lucia.gomez@farmacity.com.ar",
            ProfileType = UserProfileType.Cliente, CountryId = argentinaId,
            AssignedClientIds = new List<int> { farmacity.Id }
        };
        var clienteAr2 = new AppUser
        {
            Id = _nextUserId++, Name = "Martín Pérez", Email = "martin.perez@bancogalicia.com.ar",
            ProfileType = UserProfileType.Cliente, CountryId = argentinaId,
            AssignedClientIds = new List<int> { bancoGalicia.Id }
        };

        var internoCl1 = new AppUser
        {
            Id = _nextUserId++, Name = "Camila Rojas", Email = "camila.rojas@empresa.cl",
            ProfileType = UserProfileType.Interno, CountryId = chileId,
            HasAllBranches = true,
            ResolverGroupIds = new List<int> { mesaAyudaCl.Id }
        };
        var internoCl2 = new AppUser
        {
            Id = _nextUserId++, Name = "Diego Muñoz", Email = "diego.munoz@empresa.cl",
            ProfileType = UserProfileType.Interno, CountryId = chileId,
            HasAllBranches = false,
            AssignedBranchIds = new List<int> { brLiderProvidencia.Id, brLiderLasCondes.Id, brLiderMaipu.Id },
            ResolverGroupIds = new List<int> { logisticaCl.Id }
        };
        var internoAr1 = new AppUser
        {
            Id = _nextUserId++, Name = "Sofía Díaz", Email = "sofia.diaz@empresa.com.ar",
            ProfileType = UserProfileType.Interno, CountryId = argentinaId,
            HasAllBranches = true,
            ResolverGroupIds = new List<int> { mesaAyudaAr.Id }
        };
        var internoAr2 = new AppUser
        {
            Id = _nextUserId++, Name = "Nicolás Torres", Email = "nicolas.torres@empresa.com.ar",
            ProfileType = UserProfileType.Interno, CountryId = argentinaId,
            HasAllBranches = false,
            AssignedBranchIds = new List<int> { brGaliciaMicrocentro.Id, brGaliciaBelgrano.Id },
            ResolverGroupIds = new List<int> { seguridadAr.Id }
        };

        mesaAyudaCl.MemberUserIds.Add(internoCl1.Id);
        logisticaCl.MemberUserIds.Add(internoCl2.Id);
        mesaAyudaAr.MemberUserIds.Add(internoAr1.Id);
        seguridadAr.MemberUserIds.Add(internoAr2.Id);

        Users.AddRange(new[] { clienteCl1, clienteCl2, clienteAr1, clienteAr2, internoCl1, internoCl2, internoAr1, internoAr2 });

        // --- Casos de ejemplo (para poder ver la bandeja y las métricas con datos) ---
        var now = DateTime.UtcNow;

        AddSeedCase(chileId, "CL", lider.Id, brLiderProvidencia.Id, CaseType.Reclamo,
            "El lector de tarjetas de la caja 3 no funciona desde ayer.", clienteCl1.Id,
            now.AddDays(-6),
            new (CaseStatus, TimeSpan, int?)[]
            {
                (CaseStatus.Inicial, TimeSpan.Zero, null),
                (CaseStatus.Asignado, TimeSpan.FromHours(2), internoCl1.Id),
                (CaseStatus.EnAnalisis, TimeSpan.FromHours(10), internoCl2.Id),
                (CaseStatus.Resuelto, TimeSpan.FromHours(30), internoCl2.Id)
            },
            new (int?, TimeSpan)[] { (null, TimeSpan.Zero), (mesaAyudaCl.Id, TimeSpan.FromHours(2)), (logisticaCl.Id, TimeSpan.FromHours(10)) },
            categoryId: catFallaTecnica.Id);

        // Solicitud de cartelería para dos sucursales a la vez: un caso por sucursal.
        foreach (var branchId in new[] { brBancoChileCentro.Id, brBancoChileNunoa.Id })
        {
            AddSeedCase(chileId, "CL", bancoChile.Id, branchId, CaseType.Solicitud,
                "Solicitamos instalación de cartelería nueva.", clienteCl2.Id,
                now.AddDays(-3),
                new (CaseStatus, TimeSpan, int?)[]
                {
                    (CaseStatus.Inicial, TimeSpan.Zero, null),
                    (CaseStatus.Asignado, TimeSpan.FromHours(5), internoCl1.Id)
                },
                new (int?, TimeSpan)[] { (null, TimeSpan.Zero), (mesaAyudaCl.Id, TimeSpan.FromHours(5)) });
        }

        AddSeedCase(chileId, "CL", lider.Id, brLiderMaipu.Id, CaseType.Consulta,
            "Consulta sobre el próximo servicio de recolección de valores.", clienteCl1.Id,
            now.AddHours(-20),
            new (CaseStatus, TimeSpan, int?)[] { (CaseStatus.Inicial, TimeSpan.Zero, null) },
            new (int?, TimeSpan)[] { (null, TimeSpan.Zero) });

        // Reclamo por demora en recaudación en dos sucursales a la vez: un caso por sucursal.
        foreach (var branchId in new[] { brFarmacityPalermo.Id, brFarmacityRecoleta.Id })
        {
            AddSeedCase(argentinaId, "AR", farmacity.Id, branchId, CaseType.Reclamo,
                "Demora reiterada del servicio de recaudación.", clienteAr1.Id,
                now.AddDays(-5),
                new (CaseStatus, TimeSpan, int?)[]
                {
                    (CaseStatus.Inicial, TimeSpan.Zero, null),
                    (CaseStatus.Asignado, TimeSpan.FromHours(3), internoAr1.Id),
                    (CaseStatus.EnAnalisis, TimeSpan.FromHours(20), internoAr2.Id),
                    (CaseStatus.Resuelto, TimeSpan.FromHours(50), internoAr2.Id)
                },
                new (int?, TimeSpan)[] { (null, TimeSpan.Zero), (mesaAyudaAr.Id, TimeSpan.FromHours(3)), (seguridadAr.Id, TimeSpan.FromHours(20)) },
                categoryId: catSinMovimientoCliente.Id);
        }

        AddSeedCase(argentinaId, "AR", bancoGalicia.Id, brGaliciaMicrocentro.Id, CaseType.Solicitud,
            "Solicitud de refuerzo de seguridad para evento especial.", clienteAr2.Id,
            now.AddDays(-1),
            new (CaseStatus, TimeSpan, int?)[]
            {
                (CaseStatus.Inicial, TimeSpan.Zero, null),
                (CaseStatus.Asignado, TimeSpan.FromHours(1), internoAr1.Id)
            },
            new (int?, TimeSpan)[] { (null, TimeSpan.Zero), (seguridadAr.Id, TimeSpan.FromHours(1)) },
            categoryId: catInsumosNoEnviados.Id);

        AddSeedCase(argentinaId, "AR", farmacity.Id, brFarmacityRecoleta.Id, CaseType.Consulta,
            "Consulta por cambio de horario de atención.", clienteAr1.Id,
            now.AddHours(-8),
            new (CaseStatus, TimeSpan, int?)[] { (CaseStatus.Inicial, TimeSpan.Zero, null) },
            new (int?, TimeSpan)[] { (null, TimeSpan.Zero) });
    }

    private void AddSeedCase(
        int countryId, string countryCode, int clientId, int branchId, CaseType caseType,
        string description, int createdByUserId, DateTime createdAtUtc,
        (CaseStatus Status, TimeSpan Offset, int? ByUserId)[] statusSteps,
        (int? GroupId, TimeSpan Offset)[] groupSteps,
        int? categoryId = null)
    {
        var id = _nextCaseId++;
        var serviceCase = new ServiceCase
        {
            Id = id,
            Number = $"{countryCode}-{id:D6}",
            ClientId = clientId,
            BranchId = branchId,
            Type = caseType,
            CategoryId = categoryId,
            Description = description,
            CountryId = countryId,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc,
            Status = statusSteps[^1].Status,
            AssignedGroupId = groupSteps[^1].GroupId
        };

        foreach (var step in statusSteps)
        {
            serviceCase.StatusHistory.Add(new CaseStatusHistoryEntry
            {
                Status = step.Status,
                ChangedAtUtc = createdAtUtc.Add(step.Offset),
                ChangedByUserId = step.ByUserId
            });
        }

        foreach (var step in groupSteps)
        {
            serviceCase.GroupHistory.Add(new CaseGroupHistoryEntry
            {
                ResolverGroupId = step.GroupId,
                ChangedAtUtc = createdAtUtc.Add(step.Offset)
            });
        }

        Cases.Add(serviceCase);
    }
}
