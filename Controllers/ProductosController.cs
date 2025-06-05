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
            var productosQuery = _context.Productos.AsQueryable();

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

                // Calcular el precio de venta con el 30% de ganancia
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

        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            producto.PrecioVenta = producto.PrecioCompra * 1.30m;

            var viewModel = new ProductosViewModel
            {
                NuevoProducto = producto,
                ListaProductos = await _context.Productos
                .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductosViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.ListaProductos = await _context.Productos
                    .ToListAsync();
                return View(viewModel);
            }

            var producto = await _context.Productos.FindAsync(viewModel.NuevoProducto.Id);

            if (producto == null)
            {
                return NotFound();
            }

            try
            {
                producto.Nombre = viewModel.NuevoProducto.Nombre;
                producto.Cantidad = viewModel.NuevoProducto.Cantidad;
                producto.PrecioCompra = viewModel.NuevoProducto.PrecioCompra;
                producto.FechaVencimiento = viewModel.NuevoProducto.FechaVencimiento;
                producto.Lote = viewModel.NuevoProducto.Lote;
                producto.PrecioVenta = producto.PrecioCompra * 1.30m;
                producto.Estado = viewModel.NuevoProducto.Estado;

                _context.Update(producto);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "El producto fue actualizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al modificar el producto: {ex.Message}");
            }

            viewModel.ListaProductos = await _context.Productos.ToListAsync();
            return View(viewModel);
        }

        public IActionResult Delete(int id)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        //Eliminacion de producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deleted(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null)
            {
                return NotFound();
            }

            producto.Estado = false;
            _context.SaveChanges();
            TempData["Mensaje"] = "Producto desactivado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
