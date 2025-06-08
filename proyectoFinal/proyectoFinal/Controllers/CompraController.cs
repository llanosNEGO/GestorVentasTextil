using proyectoFinal.Data;
using proyectoFinal.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using proyectoFinal.ViewM;

namespace proyectoFinal.Controllers
{
    public class CompraController : Controller
    {
        public readonly AppDBContext _dbContext;
        public CompraController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<ComprasProveedor> lista = await _dbContext.comprasProveedores
                .Include(c => c.proveedor)
                .Include(c => c.medioPago)
                .ToListAsync();
            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var compra = await _dbContext.comprasProveedores
                    .Include(c => c.proveedor)
                    .Include(c => c.medioPago)
                    .Include(c => c.detalleCompras).ThenInclude(m => m.productos)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.idCompraProveedor == id);

                if (compra == null)
                {
                    TempData["Error"] = "Compra no Encontrada";
                    Console.WriteLine("Error al cargar la Compra");
                    return RedirectToAction(nameof(Lista));
                }
                return View(compra);

            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles de la Compra";
                Console.WriteLine($"Erorr al cargar los detalles: {ex.Message}");
                return RedirectToAction(nameof(Lista));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            try
            {
                //Cargar datos necesarios para los dropdowns
                var proveedores = await _dbContext.Proveedores.AsNoTracking().ToListAsync();
                var mediopago = await _dbContext.MedioPagos.AsNoTracking().ToListAsync();
                var productos = await _dbContext.Productos.AsNoTracking().ToListAsync();

                // Validar que existan datos necesarios
                if (!proveedores.Any())
                {
                    Console.WriteLine("Error al cargar proveedores");
                    TempData["Error"] = "Debe existir al menos un proveedor";
                    return RedirectToAction(nameof(Lista));
                }
                if (!mediopago.Any())
                {
                    Console.WriteLine("Error al cargar medio de pago");
                    TempData["Error"] = "Debe existir al menos un medio de pago";
                    return RedirectToAction(nameof(Lista));
                }
                if (!productos.Any())
                {
                    Console.WriteLine("Error al cargar productos");
                    TempData["Error"] = "Debe existir al menos un producto";

                    return RedirectToAction(nameof(Lista));
                }
                // Preparar ViewBags para los selects
                ViewBag.Productos = productos;
                ViewBag.Proveedores = new SelectList(await _dbContext.Proveedores.AsNoTracking().ToListAsync(), "idProveedor", "nombreEmpresa");
                ViewBag.MediosPago = new SelectList(await _dbContext.MedioPagos.AsNoTracking().ToListAsync(), "idMedioPago", "nombre");
                // Inicializar modelo con fecha actual
                var model = new CompraVM
                {
                    fecha_registro = DateOnly.FromDateTime(DateTime.Now),
                    detalles = new List<DetalleCompraVM>() { new DetalleCompraVM() }
                };
                return View(model);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error al cargar Nueva Compra: {ex.Message}");
                TempData["Error"] = "Error al cargar el formulario de Compra";
                return RedirectToAction(nameof(Lista));
            }
        }

        [HttpGet]
        public IActionResult ObtenerFilasDetalleC()
        {
            var productos = _dbContext.Productos.AsNoTracking().ToList();
            return PartialView("DetalleCompraRow", new DetalleCompraVM());
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerPrecioProducto(int id)
        {
            var producto = await _dbContext.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true, precio = producto.precioProducto });
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo (CompraVM model)
        {
            try
            {
                // Limpiar ModelState para propiedades de navegación
                ModelState.Remove("Detalles");

                if (ModelState.IsValid && model.detalles != null && model.detalles.Any())
                {
                    using (var transaccion = await _dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var proveedorExist = await _dbContext.Proveedores.AnyAsync(c => c.idProveedor == model.idProveedor);
                            var metodoPagoExist = await _dbContext.MedioPagos.AnyAsync(c => c.idMedioPago == model.idmedioPado);
                            if (!proveedorExist || !metodoPagoExist)
                            {
                                ModelState.AddModelError("", "Proveedor o medio de pago no válido");
                                Console.Write("Error al cargar los ID de Medio de Pago o Proveedor ");
                                throw new Exception("Datos referenciales inválidos");
                            }
                            var compra = new ComprasProveedor
                            {
                                fecha_registro = model.fecha_registro,
                                idProveedor = model.idProveedor,
                                idmedioPago = model.idmedioPado,
                                total = model.detalles.Sum(d => d.subtotal),
                                estado = true,
                                detalleCompras = new List<DetalleCompra>()
                            };

                            foreach (var detalleCvm in model.detalles)
                            {
                                var producto = await _dbContext.Productos.FindAsync(detalleCvm.idProducto);
                                if (producto == null)
                                {
                                    throw new Exception($"Producto con Id {detalleCvm.idProducto} No existe");
                                }
                                var detalle = new DetalleCompra
                                {
                                    idProducto = detalleCvm.idProducto,
                                    cantidad = detalleCvm.cantidad,
                                    color = detalleCvm.color,
                                    talla = detalleCvm.talla,
                                    precioUnitario = detalleCvm.precioUnitario,
                                    subtotal = detalleCvm.subtotal
                                };

                                compra.detalleCompras.Add(detalle);

                                producto.stock += detalleCvm.cantidad;
                            }
                            _dbContext.comprasProveedores.Add(compra);
                            await _dbContext.SaveChangesAsync();
                            await transaccion.CommitAsync();

                            Console.WriteLine("Exito al registrar una Compra");
                            TempData["Exito"] = "Compra registrada correctamente";
                            return RedirectToAction(nameof(Lista));
                        }
                        catch (Exception ex)
                        {
                            await transaccion.RollbackAsync();
                            ModelState.AddModelError("", "Error al registrar la compra: " + ex.Message);
                            Console.WriteLine($"Error en transacción: {ex.Message}");
                        }
                    }
                }
                else if (model.detalles == null || !model.detalles.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la compra");
                    Console.WriteLine("Error al cargar los productos a la compra");
                }
                // Recargar datos necesarios si hay error
                ViewBag.Proveedores = new SelectList(await _dbContext.Proveedores.AsNoTracking().ToListAsync(), "idProveedor", "nombreEmpresa");
                ViewBag.MediosPago = new SelectList(await _dbContext.MedioPagos.AsNoTracking().ToListAsync(), "idMedioPago", "descripcion");
                ViewBag.Productos = new SelectList(await _dbContext.Productos.AsNoTracking().ToListAsync(), "idProducto", "nombreProducto");

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Compra/Nuevo(POST): {ex.Message}");
                TempData["Error"] = "Error al procesar la compra";
                return RedirectToAction(nameof(Lista));
            }
        }



    }
}
