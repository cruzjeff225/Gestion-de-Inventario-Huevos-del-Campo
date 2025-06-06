using System.Security.Claims;
using Gestión_de_Inventario_Huevos_del_Campo.Db;
using Gestión_de_Inventario_Huevos_del_Campo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestión_de_Inventario_Huevos_del_Campo.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(UsuariosViewModel filtro)
        {
            var usuariosQuery = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.SearchString))
            {
                var search = filtro.SearchString.Trim().ToLower();

                usuariosQuery = usuariosQuery
                    .Where(u =>
                        u.Nombre.ToLower().Contains(search) ||
                        u.Apellido.ToLower().Contains(search) ||
                        u.CorreoElectronico.ToLower().Contains(search));
            }

            var usuarios = await usuariosQuery.ToListAsync();

            filtro.ListaUsuarios = usuarios;
            filtro.NuevoUsuario = new Usuarios();

            return View(filtro);
        }

        public IActionResult Create()
        {
            return View(new UsuariosViewModel { NuevoUsuario = new Usuarios()});
        }

        // Guardar nuevo usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuariosViewModel viewModel)
        {

            viewModel.NuevoUsuario.FechaCreacion = DateTime.Now;

            if (!ModelState.IsValid)
            {
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
                viewModel.ListaUsuarios = await _context.Usuarios.ToListAsync();
                return View("Index", viewModel);
            }

            try
            {

                _context.Usuarios.Add(viewModel.NuevoUsuario);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Usuario agregado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar el usuario: {ex.Message}");
                viewModel.ListaUsuarios = await _context.Usuarios.ToListAsync();
                return View("Index", viewModel);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var viewModel = new UsuariosViewModel
            {
                NuevoUsuario = usuario,
                ListaUsuarios = await _context.Usuarios.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuariosViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.ListaUsuarios = await _context.Usuarios.ToListAsync();
                return View(viewModel);
            }

            var usuario = await _context.Usuarios.FindAsync(viewModel.NuevoUsuario.ID);

            if (usuario == null)
            {
                return NotFound();
            }

            try
            {
                usuario.Nombre = viewModel.NuevoUsuario.Nombre;
                usuario.Apellido = viewModel.NuevoUsuario.Apellido;
                usuario.CorreoElectronico = viewModel.NuevoUsuario.CorreoElectronico;
                usuario.Contraseña = viewModel.NuevoUsuario.Contraseña;
                // FechaCreacion no se modifica

                _context.Update(usuario);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "El usuario fue actualizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al modificar el usuario: {ex.Message}");
            }

            viewModel.ListaUsuarios = await _context.Usuarios.ToListAsync();
            return View(viewModel);
        }

        public IActionResult Delete(int id)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.ID == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // Eliminación lógica de usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deleted(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
            TempData["Mensaje"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }


        // Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.CorreoElectronico == model.CorreoElectronico && u.Contraseña == model.Contraseña);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.NameIdentifier, usuario.ID.ToString()),
                new Claim(ClaimTypes.Email, usuario.CorreoElectronico)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // Logout
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuarios");
        }
    }
}