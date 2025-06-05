using proyectoFinal.Data;
using proyectoFinal.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using proyectoFinal.ViewM;

namespace proyectoFinal.Controllers
{
    public class ClienteController : Controller
    {
        private readonly AppDBContext _appDBContext;
        public ClienteController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<Cliente> lista = await _appDBContext.Clientes.ToListAsync();
            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Nuevo(Cliente cliente)
        {
            try
            {
                // Limpiar validación para propiedades de navegación
                ModelState.Remove("ventas");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== ERRORES DE VALIDACIÓN ===");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"Campo: {error.Key}");
                        foreach (var errorMsg in error.Value.Errors)
                        {
                            Console.WriteLine($"  Error: {errorMsg.ErrorMessage}");
                        }
                    }
                    return await RecargarFormulario(cliente);
                }

                // Verificar si el cliente YA existe (no debería existir para crear uno nuevo)
                var clienteExistente = await _appDBContext.Clientes
                    .AnyAsync(c => c.idCliente == cliente.idCliente);

                if (clienteExistente)
                {
                    ModelState.AddModelError("idCliente", "Ya existe un cliente con este ID");
                    return await RecargarFormulario(cliente);
                }

                // Crear nuevo cliente
                Cliente nuevoCliente = new Cliente
                {
                    nombreCliente = cliente.nombreCliente,
                    telefono = cliente.telefono,
                    email = cliente.email
                };

                _appDBContext.Clientes.Add(nuevoCliente);
                await _appDBContext.SaveChangesAsync();

                TempData["Exito"] = "Cliente creado correctamente";
                return RedirectToAction(nameof(Lista));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Nuevo(POST): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                TempData["Error"] = "Error al guardar el cliente";
                return await RecargarFormulario(cliente);
            }
        }
        private async Task<IActionResult> RecargarFormulario(Cliente cliente)
        {
            // Reutilizar lógica de carga como en GET
            ViewBag.Categorias = new SelectList(
                await _appDBContext.Categorias.AsNoTracking().ToListAsync(),
                nameof(Categoria.idCategoria),
                nameof(Categoria.nombreCategoria));

            return View(cliente);
        }



    }
}
