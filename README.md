# Gestión de Casos (módulo de prueba)

Proyecto ASP.NET Core MVC (.NET 8) en blanco con un nuevo módulo para registrar y
gestionar **solicitudes de servicio, consultas y reclamos**, pensado para integrarse
a una plataforma existente (por eso no incluye login propio).

## Estructura

```
GestionCasos.sln
src/GestionCasos.Web/
  Models/        Entidades del dominio (Country, Client, Branch, Category,
                 ResolverGroup, AppUser, ServiceCase, historiales de estado/grupo)
  Data/          InMemoryDataStore: almacén en memoria + datos de ejemplo (seed)
  Services/      Lógica de negocio (CurrentUserService, CatalogService,
                 CaseService, MetricsService)
  Controllers/   Home, Session, Cases, InternalCases, Categories,
                 ResolverGroups, Clients, Users, Metrics
  Views/         Razor views, layout con sidebar/topbar
  wwwroot/       CSS y JS propios (sin dependencias externas)
```

No usa base de datos real todavía: todo vive en `InMemoryDataStore` (singleton).
Cambiar a una base real implica reemplazar esa clase y las implementaciones de
`ICatalogService` / `ICaseService` / `IMetricsService` — el resto del código
(controllers, vistas) no cambia porque sólo depende de esas interfaces.

## Cómo correrlo

Requiere el SDK de .NET 8 (o superior, ya que el proyecto tiene
`RollForward=LatestMajor` para poder ejecutarse también sobre un runtime más nuevo
si sólo ese está instalado).

```bash
cd src/GestionCasos.Web
dotnet run
```

La app levanta en `http://localhost:5080` (ver `Properties/launchSettings.json`).

## Sin login: selector de usuario de prueba

Esta plataforma existente ya resuelve el login; este módulo sólo necesita saber,
para el usuario ya autenticado, su **perfil** (Cliente / Interno), su **país**
(Chile o Argentina) y su alcance (clientes o sucursales asignadas).

Mientras no está integrado con el login real, `/Session/Switch` permite elegir
un usuario de prueba (persiste en una cookie). El punto de integración real es
`Services/ICurrentUserService` — para conectarlo a la plataforma existente basta
con reemplazar `CurrentUserService` por una implementación que resuelva el
usuario desde la sesión/claims que ya use la plataforma, y eliminar
`SessionController`.

## Modelo funcional

- **País**: Chile y Argentina. Usuarios, clientes y grupos resolutores están
  divididos por país.
- **Perfil Cliente**: tiene uno o más *clientes* asociados; cada cliente tiene
  una lista de *sucursales/puntos*. Al crear un caso elige el cliente, una o
  varias sucursales, una *categoría* (Solicitud, Consulta o Reclamo, más las
  que el equipo interno agregue) y un detalle en texto libre. Si selecciona
  varias sucursales a la vez, se genera **un caso independiente por cada una**
  (mismo detalle y categoría, pero seguimiento y estado propios).
- **Perfil Interno**: tiene asociadas una, varias o todas las sucursales del
  país. Ve en su bandeja los casos de esas sucursales, puede agruparlos o
  filtrarlos por categoría/estado/grupo, y derivarlos a un **grupo resolutor**
  con un comentario u observación opcional (queda en el historial del caso).
- **Grupos resolutores**: ABM de grupos por país, con usuarios internos como
  miembros; un caso derivado a un grupo pasa automáticamente de "Inicial" a
  "Asignado".
- **Categorías**: Solicitud/Consulta/Reclamo vienen de fábrica (no se pueden
  borrar); el perfil interno puede agregar más.
- **Estados de un caso**: Inicial, Asignado, En análisis, Resuelto. Cada cambio
  de estado y cada derivación a grupo queda en el historial del caso (con quién
  y cuándo), que es la base para las métricas.
- **Métricas** (`/Metrics`, sólo perfil Interno): casos iniciados, resueltos y
  pendientes; tiempo promedio de resolución; tiempo promedio de un grupo
  resolutor a otro; tiempo promedio para pasar de un estado al siguiente;
  distribución por estado y por categoría. Filtrable por país y categoría.

## Datos de ejemplo

El seed incluye 4 clientes (2 por país), 9 sucursales, 4 grupos resolutores,
4 usuarios Cliente y 4 usuarios Interno, y 6 casos de ejemplo en distintos
estados para poder ver la bandeja y el panel de métricas con datos desde el
primer arranque.
