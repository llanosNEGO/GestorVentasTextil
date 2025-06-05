using proyectoFinal.Data;
using proyectoFinal.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;

namespace proyectoFinal.Controllers
{
    public class InventarioController : Controller
    {
        private readonly AppDBContext _dbContext;
        public InventarioController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> Lista(int id)
        {
            try
            {
                // 1. Verificar si el producto existe
                var producto = await _dbContext.Productos
                    .Include(p => p.inventario) // Incluir los inventarios relacionados
                    .FirstOrDefaultAsync(p => p.IdProducto == id);

                if (producto == null)
                {
                    TempData["Error"] = "Producto no encontrado";
                    return RedirectToAction("Lista", "Producto");
                }

                // 2. Obtener el inventario del producto
                var inventario = await _dbContext.Inventarios
                    .Where(i => i.idProducto == id)
                    .ToListAsync();

                // 3. Pasar datos a la vista
                ViewBag.Producto = producto;
                return View(inventario);
            }
            catch (Exception ex)
            {
                // Log del error (puedes implementar un sistema de logging más robusto)
                Console.WriteLine($"Error en Inventario/Lista: {ex.Message}");
                TempData["Error"] = "Error al cargar el inventario";
                return RedirectToAction("Lista", "Producto");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Agregar(int idProducto)
        {
            try
            {
                // Verificar si el producto existe
                var producto = await _dbContext.Productos
                    .FirstOrDefaultAsync(p => p.IdProducto == idProducto);

                if (producto == null)
                {
                    TempData["Error"] = "Producto no encontrado";
                    return RedirectToAction("Lista", "Producto");
                }

                ViewBag.Producto = producto;
                return View(new Inventario { idProducto = idProducto });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Inventario/Agregar(GET): {ex.Message}");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction("Lista", new { id = idProducto });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(Inventario inventario)
        {
            try
            {
                // Remover errores de navegación
                ModelState.Remove("producto");

                if (ModelState.IsValid)
                {
                    // Verificar que el producto exista
                    var productoExistente = await _dbContext.Productos
                        .AnyAsync(p => p.IdProducto == inventario.idProducto);

                    if (!productoExistente)
                    {
                        TempData["Error"] = "Producto no válido";
                        return RedirectToAction("Lista", "Producto");
                    }

                    // Verificar si ya existe un inventario con la misma talla y color
                    var existeVariacion = await _dbContext.Inventarios
                        .AnyAsync(i => i.idProducto == inventario.idProducto
                                    && i.talla == inventario.talla
                                    && i.color == inventario.color);

                    if (existeVariacion)
                    {
                        TempData["Error"] = "Ya existe un registro con esta talla y color para este producto";
                        return RedirectToAction("Agregar", new { idProducto = inventario.idProducto });
                    }

                    _dbContext.Inventarios.Add(inventario);
                    await _dbContext.SaveChangesAsync();

                    TempData["Exito"] = "Variación agregada al inventario correctamente";
                    return RedirectToAction("Lista", new { id = inventario.idProducto });
                }

                // Si el modelo no es válido, recargar el formulario
                var producto = await _dbContext.Productos
                    .FirstOrDefaultAsync(p => p.IdProducto == inventario.idProducto);

                if (producto == null)
                {
                    TempData["Error"] = "Producto no encontrado";
                    return RedirectToAction("Lista", "Producto");
                }

                ViewBag.Producto = producto;
                return View(inventario);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Inventario/Agregar(POST): {ex.Message}");
                TempData["Error"] = "Error al guardar la variación del inventario";
                return RedirectToAction("Lista", new { id = inventario.idProducto });
            }
        }
    }
}
