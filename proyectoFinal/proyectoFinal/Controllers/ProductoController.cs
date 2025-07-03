using proyectoFinal.Data;
using proyectoFinal.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace proyectoFinal.Controllers
{
    [Authorize(Roles = "Administrador, Usuario")]
    public class ProductoController : Controller
    {
        private readonly AppDBContext _dbContext;
        public ProductoController(AppDBContext appDBContext)
        {
            _dbContext = appDBContext;
            InicializarMedioPago().Wait();
            InicializarCategorias().Wait();
        }

        private async Task InicializarMedioPago()
        {
            if (!await _dbContext.MedioPagos.AnyAsync())
            {
                var mediopago = new List<MedioPago>
                {
                    new MedioPago { nombre = "Efectivo" },
                    new MedioPago { nombre = "Yape" }
                };
                await _dbContext.MedioPagos.AddRangeAsync(mediopago);
                await _dbContext.SaveChangesAsync();
            }
        }
        private async Task InicializarCategorias()
        {
            if(! await _dbContext.Categorias.AnyAsync())
            {
                var categorias = new List<Categoria>
                {
                    new Categoria {nombreCategoria = "Sintetico", descripcion ="Material mas barato" },
                    new Categoria {nombreCategoria ="Algodon" , descripcion="Material de algodon"}
                };
                await _dbContext.Categorias.AddRangeAsync(categorias);
                await _dbContext.SaveChangesAsync();
            }
        }

        [HttpGet]

        public async Task<IActionResult> Lista()
        {
            List<Producto> lista = await _dbContext.Productos.ToListAsync();
            return View(lista);
        }


        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            try
            {
                // 1. Cargar categorías con Include (como en Detalle())
                var categorias = await _dbContext.Categorias
                    .AsNoTracking()
                    .ToListAsync();

                // 2. Validación robusta (como en Inicio())
                if (!categorias.Any())
                {
                    TempData["Error"] = "No existen categorías. Crea al menos una primero.";
                    return RedirectToAction("Lista");
                }

                // 3. SelectList seguro (patrón probado en Playlist)
                ViewBag.Categorias = new SelectList(categorias, "idCategoria", "nombreCategoria");

                // 4. Modelo inicializado (evita null reference)
                return View(new Producto());
            }
            catch (Exception ex)
            {
                // Log de errores (mejor práctica observada)
                Console.WriteLine($"Error en Nuevo(GET): {ex.Message}");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction("Lista");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Producto producto)
        {
            try
            {
                // SOLUCIÓN: Remover errores de propiedades de navegación del ModelState
                ModelState.Remove("categoria");
                ModelState.Remove("inventario");
                ModelState.Remove("detalleVentas");
                ModelState.Remove("detalleCompras");
                ModelState.Remove("detallePedidos");

                // Debug del ModelState después de limpiar
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== ERRORES DE VALIDACIÓN (después de limpiar) ===");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"Campo: {error.Key}");
                        foreach (var errorMsg in error.Value.Errors)
                        {
                            Console.WriteLine($"  Error: {errorMsg.ErrorMessage}");
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    var categoriaExistente = await _dbContext.Categorias
                        .AnyAsync(c => c.idCategoria == producto.idCategoria);

                    if (!categoriaExistente)
                    {
                        ModelState.AddModelError("idCategoria", "Categoría no válida");
                        Console.WriteLine("Error de Categoria");
                        return await RecargarFormulario(producto);
                    }

                    Producto producto1 = new Producto
                    {
                        nombreProducto = producto.nombreProducto,
                        descripcionProducto = producto.descripcionProducto,
                        precioProducto = producto.precioProducto,
                        stock = producto.stock,
                        idCategoria = producto.idCategoria,
                        ImagenUrl = producto.ImagenUrl ?? "https://www.shutterstock.com/image-vector/no-photo-available-vector-icon-260nw-2082597646.jpg"
                    };

                    _dbContext.Productos.Add(producto1);
                    await _dbContext.SaveChangesAsync();

                    TempData["Exito"] = "Producto creado correctamente";
                    Console.WriteLine("¡Producto creado exitosamente!");
                    return RedirectToAction(nameof(Lista));
                }

                Console.WriteLine($"Datos recibidos: Nombre={producto.nombreProducto},Descripcion={producto.descripcionProducto}, Precio={producto.precioProducto}, Categoría={producto.idCategoria}");
                Console.WriteLine("Error de formulario - ModelState aún no válido");
                return await RecargarFormulario(producto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Nuevo(POST): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                TempData["Error"] = "Error al guardar el producto";
                return await RecargarFormulario(producto);
            }
        }
        private async Task<IActionResult> RecargarFormulario(Producto producto)
        {
            // Reutilizar lógica de carga como en GET
            ViewBag.Categorias = new SelectList(
                await _dbContext.Categorias.AsNoTracking().ToListAsync(),
                nameof(Categoria.idCategoria),
                nameof(Categoria.nombreCategoria));

            return View(producto);
        }

    }
}
