using Gestión_de_Inventario_Huevos_del_Campo.Db;
using Gestión_de_Inventario_Huevos_del_Campo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestión_de_Inventario_Huevos_del_Campo.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(ProductosViewModel filtro)
        {
            var productosQuery = _context.Productos
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.SearchString))
            {
                var search = filtro.SearchString.Trim().ToLower();

                productosQuery = productosQuery
                    .Where(p =>
                        p.Nombre.ToLower().Contains(search));
            }

            var productos = await productosQuery.ToListAsync();

            filtro.ListaProductos = productos;
            filtro.NuevoProducto = new Productos();

            return View(filtro);
        } 

        public IActionResult Create()
        { 
            return View();
        }

        // Guardar nuevo producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductosViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Imprimir el estado del modelo para depuración
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        Console.WriteLine($"Campo: {key}");
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($" - Error: {error.ErrorMessage}");
                        }
                    }
                }
            }

            try
            {
                viewModel.NuevoProducto.FechaRegistro = DateTime.Now; // Se asigna la fecha actual al registrar

                // 👉 Calcular el precio de venta con el 30% de ganancia
                viewModel.NuevoProducto.PrecioVenta = viewModel.NuevoProducto.PrecioCompra * 1.30m;

                _context.Productos.Add(viewModel.NuevoProducto);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Producto Agregado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar el producto: {ex.Message}");
                viewModel.ListaProductos = await _context.Productos
                    .ToListAsync();

                return View("Index", viewModel);
            }
        }
    }
}
