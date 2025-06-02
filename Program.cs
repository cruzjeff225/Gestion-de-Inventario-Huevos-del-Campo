using Gestión_de_Inventario_Huevos_del_Campo.Db;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure; 

var builder = WebApplication.CreateBuilder(args);


QuestPDF.Settings.License = LicenseType.Community;

// Agregar servicios
builder.Services.AddControllersWithViews();

// Configurar conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

