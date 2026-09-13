using GestionCasos.Web.Data;
using GestionCasos.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Almacén de datos en memoria (sin base de datos real todavía): un único singleton
// compartido por todos los servicios. Ver Data/InMemoryDataStore.cs.
builder.Services.AddSingleton<InMemoryDataStore>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
