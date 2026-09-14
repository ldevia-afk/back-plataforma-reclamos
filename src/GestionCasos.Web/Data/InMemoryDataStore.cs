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
    public List<SentEmail> SentEmails { get; } = new();

    private int _nextClientId = 1;
    private int _nextBranchId = 1;
    private int _nextCategoryId = 1;
    private int _nextResolverGroupId = 1;
    private int _nextUserId = 1;
    private int _nextCaseId = 1;
    private int _nextSentEmailId = 1;

    public object Lock => _lock;

    public int NextClientId() { lock (_lock) return _nextClientId++; }
    public int NextBranchId() { lock (_lock) return _nextBranchId++; }
    public int NextCategoryId() { lock (_lock) return _nextCategoryId++; }
    public int NextResolverGroupId() { lock (_lock) return _nextResolverGroupId++; }
    public int NextUserId() { lock (_lock) return _nextUserId++; }
    public int NextCaseId() { lock (_lock) return _nextCaseId++; }
    public int NextSentEmailId() { lock (_lock) return _nextSentEmailId++; }

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
        var lider = new Client { Id = _nextClientId++, Name = "Supermercados Líder", CountryId = chileId, ExternalCode = "1001" };
        var bancoChile = new Client { Id = _nextClientId++, Name = "Banco de Chile", CountryId = chileId, ExternalCode = "1002" };
        var farmacity = new Client { Id = _nextClientId++, Name = "Farmacity", CountryId = argentinaId, ExternalCode = "2001" };
        var bancoGalicia = new Client { Id = _nextClientId++, Name = "Banco Galicia", CountryId = argentinaId, ExternalCode = "2002" };
        Clients.AddRange(new[] { lider, bancoChile, farmacity, bancoGalicia });

        // --- Sucursales ---
        var brLiderProvidencia = new Branch { Id = _nextBranchId++, Name = "Líder Providencia", ClientId = lider.Id, CountryId = chileId, Address = "Av. Providencia 2124, Providencia, Santiago" };
        var brLiderLasCondes = new Branch { Id = _nextBranchId++, Name = "Líder Las Condes", ClientId = lider.Id, CountryId = chileId, Address = "Av. Apoquindo 4501, Las Condes, Santiago" };
        var brLiderMaipu = new Branch { Id = _nextBranchId++, Name = "Líder Maipú", ClientId = lider.Id, CountryId = chileId, Address = "Av. Pajaritos 3030, Maipú, Santiago" };
        var brBancoChileCentro = new Branch { Id = _nextBranchId++, Name = "Sucursal Centro", ClientId = bancoChile.Id, CountryId = chileId, Address = "Ahumada 251, Santiago Centro" };
        var brBancoChileNunoa = new Branch { Id = _nextBranchId++, Name = "Sucursal Ñuñoa", ClientId = bancoChile.Id, CountryId = chileId, Address = "Av. Irarrázaval 3125, Ñuñoa, Santiago" };
        var brFarmacityPalermo = new Branch { Id = _nextBranchId++, Name = "Farmacity Palermo", ClientId = farmacity.Id, CountryId = argentinaId, Address = "Av. Santa Fe 3253, Palermo, CABA" };
        var brFarmacityRecoleta = new Branch { Id = _nextBranchId++, Name = "Farmacity Recoleta", ClientId = farmacity.Id, CountryId = argentinaId, Address = "Av. Callao 1200, Recoleta, CABA" };
        var brGaliciaMicrocentro = new Branch { Id = _nextBranchId++, Name = "Sucursal Microcentro", ClientId = bancoGalicia.Id, CountryId = argentinaId, Address = "Florida 401, Microcentro, CABA" };
        var brGaliciaBelgrano = new Branch { Id = _nextBranchId++, Name = "Sucursal Belgrano", ClientId = bancoGalicia.Id, CountryId = argentinaId, Address = "Av. Cabildo 2040, Belgrano, CABA" };
        Branches.AddRange(new[]
        {
            brLiderProvidencia, brLiderLasCondes, brLiderMaipu,
            brBancoChileCentro, brBancoChileNunoa,
            brFarmacityPalermo, brFarmacityRecoleta,
            brGaliciaMicrocentro, brGaliciaBelgrano
        });

        // --- Grupos resolutores ---
        var mesaAyudaCl = new ResolverGroup { Id = _nextResolverGroupId++, Name = "Mesa de Ayuda CL", CountryId = chileId, IsHelpDesk = true };
        var logisticaCl = new ResolverGroup { Id = _nextResolverGroupId++, Name = "Logística CL", CountryId = chileId };
        var mesaAyudaAr = new ResolverGroup { Id = _nextResolverGroupId++, Name = "Mesa de Ayuda AR", CountryId = argentinaId, IsHelpDesk = true };
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

        // --- Historial de alta / membresía a grupos, para poder ver el historial de cada usuario ---
        foreach (var user in Users)
        {
            var createdAt = now.AddDays(user.ProfileType == UserProfileType.Cliente ? -90 : -120);
            user.History.Add(new UserHistoryEntry { Type = UserHistoryEventType.Created, OccurredAtUtc = createdAt });
            foreach (var groupId in user.ResolverGroupIds)
            {
                user.History.Add(new UserHistoryEntry { Type = UserHistoryEventType.AddedToGroup, OccurredAtUtc = createdAt.AddDays(1), ResolverGroupId = groupId });
            }
        }

        var casoProvidencia = AddSeedCase(chileId, "CL", lider.Id, brLiderProvidencia.Id, CaseType.Reclamo,
            "El lector de tarjetas de la caja 3 no funciona desde ayer.", clienteCl1.Id,
            now.AddDays(-6),
            new (CaseStatus, TimeSpan, int?)[]
            {
                (CaseStatus.Inicial, TimeSpan.Zero, null),
                (CaseStatus.Asignado, TimeSpan.FromHours(2), internoCl1.Id),
                (CaseStatus.EnAnalisis, TimeSpan.FromHours(10), internoCl2.Id),
                (CaseStatus.Resuelto, TimeSpan.FromHours(30), internoCl2.Id)
            },
            new (int?, TimeSpan)[]
            {
                (null, TimeSpan.Zero), (mesaAyudaCl.Id, TimeSpan.FromHours(2)), (logisticaCl.Id, TimeSpan.FromHours(10)),
                (mesaAyudaCl.Id, TimeSpan.FromHours(30))
            },
            categoryId: catFallaTecnica.Id,
            resolutionComment: "Se reemplazó el lector de tarjetas de la caja 3. Quedó probado y funcionando con normalidad.",
            resolutionConfirmed: true,
            resolutionConfirmedByUserId: internoCl1.Id);

        SentEmails.Add(new SentEmail
        {
            Id = _nextSentEmailId++,
            CaseId = casoProvidencia.Id,
            ToEmail = clienteCl1.Email,
            Subject = $"Resolución del caso {casoProvidencia.Number}",
            Body = BuildResolutionEmailBody(casoProvidencia, brLiderProvidencia.Name, casoProvidencia.ResolutionComment!),
            SentAtUtc = casoProvidencia.ResolutionConfirmedAtUtc!.Value
        });

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
        // Quedan Resueltos pero todavía sin confirmar por Mesa de Ayuda AR (demuestra la cola pendiente).
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
                new (int?, TimeSpan)[]
                {
                    (null, TimeSpan.Zero), (mesaAyudaAr.Id, TimeSpan.FromHours(3)), (seguridadAr.Id, TimeSpan.FromHours(20)),
                    (mesaAyudaAr.Id, TimeSpan.FromHours(50))
                },
                categoryId: catSinMovimientoCliente.Id,
                resolutionComment: "Se reforzó la frecuencia de recaudación en la sucursal. Quedamos atentos a que no vuelva a repetirse.");
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

    private ServiceCase AddSeedCase(
        int countryId, string countryCode, int clientId, int branchId, CaseType caseType,
        string description, int createdByUserId, DateTime createdAtUtc,
        (CaseStatus Status, TimeSpan Offset, int? ByUserId)[] statusSteps,
        (int? GroupId, TimeSpan Offset)[] groupSteps,
        int? categoryId = null,
        string? resolutionComment = null,
        bool resolutionConfirmed = false,
        int? resolutionConfirmedByUserId = null)
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
            AssignedGroupId = groupSteps[^1].GroupId,
            ResolutionComment = resolutionComment,
            ResolutionConfirmed = resolutionConfirmed
        };

        for (var i = 0; i < statusSteps.Length; i++)
        {
            var step = statusSteps[i];
            serviceCase.History.Add(new CaseHistoryEntry
            {
                EventType = i == 0 ? CaseEventType.Created : CaseEventType.StatusChanged,
                Status = step.Status,
                OccurredAtUtc = createdAtUtc.Add(step.Offset),
                ChangedByUserId = i == 0 ? createdByUserId : step.ByUserId
            });
        }

        foreach (var step in groupSteps.Where(s => s.GroupId.HasValue))
        {
            serviceCase.History.Add(new CaseHistoryEntry
            {
                EventType = CaseEventType.GroupAssigned,
                ResolverGroupId = step.GroupId,
                OccurredAtUtc = createdAtUtc.Add(step.Offset)
            });
        }

        if (resolutionConfirmed)
        {
            var confirmedAt = serviceCase.History.Last(h => h.EventType == CaseEventType.StatusChanged || h.EventType == CaseEventType.Created).OccurredAtUtc.AddHours(1);
            serviceCase.ResolutionConfirmedAtUtc = confirmedAt;
            serviceCase.ResolutionConfirmedByUserId = resolutionConfirmedByUserId;
            serviceCase.History.Add(new CaseHistoryEntry
            {
                EventType = CaseEventType.ResolutionConfirmed,
                OccurredAtUtc = confirmedAt,
                ChangedByUserId = resolutionConfirmedByUserId,
                Comment = resolutionComment
            });
        }

        Cases.Add(serviceCase);
        return serviceCase;
    }

    private static string BuildResolutionEmailBody(ServiceCase serviceCase, string branchName, string resolutionComment)
    {
        var startDate = serviceCase.CreatedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        var endDate = serviceCase.History.Last(h => h.Status == CaseStatus.Resuelto).OccurredAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        return
            $"Caso: {serviceCase.Number}\n" +
            $"Sucursal: {branchName}\n" +
            $"Fecha de inicio: {startDate}\n" +
            $"Fecha de finalización: {endDate}\n" +
            $"Estado final: {serviceCase.Status.ToDisplayName()}\n\n" +
            $"Comentario de resolución:\n{resolutionComment}";
    }
}
