using proyectoFinal.Data;
using proyectoFinal.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using proyectoFinal.ViewM;
using Microsoft.AspNetCore.Authorization;

namespace proyectoFinal.Controllers
{
    [Authorize(Roles = "Administrador")]
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
                ModelState.Remove("pedidos");
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

        //Acion Eliminar 
        [HttpGet]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) { return NotFound(); }

            var cliente = await _appDBContext.Clientes.FirstOrDefaultAsync(d => d.idCliente == id);

            if (cliente == null) { return NotFound(); }

            return View(cliente);
        }
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado( int id)
        {
            try
            {
                var cliente = await _appDBContext.Clientes.FindAsync(id);
                if (cliente == null) { return NotFound(); }

                bool tieneventas = await _appDBContext.Ventas.AnyAsync(v => v.idCliente == id);

                if (tieneventas) { 
                    Console.WriteLine("No se puede eliminar el Cliente porque tiene venta asociada");
                    TempData["Error"] = "No se puede eliminar el cliente porque tiene ventas asociadas";
                    return RedirectToAction(nameof(Lista)); 
                }

                _appDBContext.Clientes.Remove(cliente);
                await _appDBContext.SaveChangesAsync();
                TempData["Exito"] = "Cliente eliminado correctamente";
                Console.WriteLine("Eliminacion Completada de CLiente");
                return RedirectToAction(nameof(Lista));
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error al ejecutar Eliminar { ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return RedirectToAction(nameof(Lista));
            }
        }

        //Modulo de Editar

        [HttpGet]
        public async Task<IActionResult> Editar (int? id)
        {
            if (id == null){ return NotFound(); }

            var cliente = await _appDBContext.Clientes.FindAsync(id);

            if (cliente == null) { return NotFound(); }
            return View(cliente);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Cliente cliente)
        {
            if(id != cliente.idCliente) { return NotFound(); }

            try
            {
                ModelState.Remove("Ventas");

                if (ModelState.IsValid)
                {
                    _appDBContext.Update(cliente);
                    await _appDBContext.SaveChangesAsync();

                    Console.WriteLine("Cliente editado correctamente");
                    return RedirectToAction(nameof(Lista));
                }
                else
                {
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.Write($"Error : {error.ErrorMessage}");
                    }
                    return View(cliente);
                }


            }catch( DbUpdateConcurrencyException ex)
            {
                
                if (!ClienteExists(cliente.idCliente))
                {
                    return NotFound();
                }
                else { Console.Write($"Error de Concurrencia {ex.Message}"); return View(cliente); }
            }
            catch(Exception ex)
            {
                Console.Write($"Error: {ex.Message}");
                return View(cliente);
            }


        }
        private bool ClienteExists(int id)
        {
            return _appDBContext.Clientes.Any(e => e.idCliente == id);
        }


    }
}
