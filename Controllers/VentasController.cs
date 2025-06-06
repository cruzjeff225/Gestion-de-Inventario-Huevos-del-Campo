using Gestión_de_Inventario_Huevos_del_Campo.Db;
using Gestión_de_Inventario_Huevos_del_Campo.Models;
using Gestión_de_Inventario_Huevos_del_Campo.PDF; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Microsoft.AspNetCore.Authorization;

namespace Gestión_de_Inventario_Huevos_del_Campo.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Mostrar formulario para nueva venta
        public IActionResult Create()
        {
            ViewBag.Productos = _context.Productos
                .Where(p => p.Estado && p.Cantidad > 0)
                .ToList();

            return View();
        }

        // Guardar venta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Cliente, int[] productoIds, int[] cantidades)
        {
            ViewBag.Productos = _context.Productos.Where(p => p.Estado && p.Cantidad > 0).ToList();

            if (string.IsNullOrWhiteSpace(Cliente) || Cliente.Trim().Length < 3)
                ModelState.AddModelError("Cliente", "Debe ingresar un nombre de cliente válido (mínimo 3 caracteres).");

            if (productoIds == null || cantidades == null || productoIds.Length != cantidades.Length)
                ModelState.AddModelError("", "Error en los productos enviados.");

            if (productoIds != null && productoIds.Length == 0)
                ModelState.AddModelError("", "Debe seleccionar al menos un producto.");

            if (cantidades != null && cantidades.Any(c => c <= 0))
                ModelState.AddModelError("", "Todas las cantidades deben ser mayores a 0.");

            decimal total = 0;
            var detalles = new List<DetalleVenta>();

            if (productoIds != null && cantidades != null)
            {
                for (int i = 0; i < productoIds.Length; i++)
                {
                    var producto = await _context.Productos.FindAsync(productoIds[i]);

                    if (producto == null)
                    {
                        ModelState.AddModelError("", $"Producto con ID {productoIds[i]} no encontrado.");
                        continue;
                    }

                    if (producto.Cantidad < cantidades[i])
                    {
                        ModelState.AddModelError("", $"No hay suficiente stock del producto: {producto.Nombre}");
                        continue;
                    }

                    decimal subtotal = producto.PrecioVenta * cantidades[i];

                    total += subtotal;

                    detalles.Add(new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = cantidades[i],
                        PrecioUnitario = producto.PrecioVenta,
                        Subtotal = subtotal
                    });

                    producto.Cantidad -= cantidades[i];
                }
            }


            if (!ModelState.IsValid)
                return View("Create", new Venta { Cliente = Cliente });

            var venta = new Venta
            {
                Cliente = Cliente.Trim(),
                Fecha = DateTime.Now,
                Total = total,
                Detalles = detalles
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Venta registrada correctamente.";
            return RedirectToAction("Index");
        }



        // Anular una venta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null || venta.Anulada)
            {
                return NotFound();
            }

            foreach (var detalle in venta.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.Cantidad += detalle.Cantidad;
                }
            }

            venta.Anulada = true;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Venta anulada correctamente.";
            return RedirectToAction("Index");
        }

        // Listar ventas
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventas);
        }

        // Ver detalle de una venta
        public async Task<IActionResult> Details(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound();

            return View(venta);
        }

        // Exportar ventas a PDF 
        [HttpGet]
        public async Task<IActionResult> ExportarPdf()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            var reporte = new ReporteVentasPdf(ventas, "Reporte de Ventas");
            var pdfBytes = reporte.GeneratePdf();

            return File(pdfBytes, "application/pdf", "ReporteVentas.pdf");
        }
    }
}
