using Gestión_de_Inventario_Huevos_del_Campo.Db;
using Gestión_de_Inventario_Huevos_del_Campo.Models;
using Gestión_de_Inventario_Huevos_del_Campo.PDF; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace Gestión_de_Inventario_Huevos_del_Campo.Controllers
{
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
        public async Task<IActionResult> Create([FromForm] Venta venta, [FromForm] int[] productoIds, [FromForm] int[] cantidades)
        {
            if (productoIds.Length != cantidades.Length)
            {
                ModelState.AddModelError("", "Error en los productos enviados.");
                ViewBag.Productos = _context.Productos.Where(p => p.Estado && p.Cantidad > 0).ToList();
                return View();
            }

            decimal total = 0;
            var detalles = new List<DetalleVenta>();

            for (int i = 0; i < productoIds.Length; i++)
            {
                var producto = await _context.Productos.FindAsync(productoIds[i]);
                if (producto == null || producto.Cantidad < cantidades[i])
                {
                    ModelState.AddModelError("", $"No hay suficiente stock del producto: {producto?.Nombre ?? "Desconocido"}");
                    ViewBag.Productos = _context.Productos.Where(p => p.Estado && p.Cantidad > 0).ToList();
                    return View();
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

            venta.Fecha = DateTime.Now;
            venta.Total = total;
            venta.Detalles = detalles;

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

        // Exportar ventas a PDF con QuestPDF
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
