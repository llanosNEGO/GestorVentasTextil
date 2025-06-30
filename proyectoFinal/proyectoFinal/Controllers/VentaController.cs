using proyectoFinal.Data;
using proyectoFinal.Models;
using proyectoFinal.ViewM;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;
using proyectoFinal.ViewM;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace proyectoFinal.Controllers
{
    public class VentaController : Controller
    {
        private readonly AppDBContext _dbContext;
        public VentaController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<Venta> lista = await _dbContext.Ventas
                .Include(v => v.cliente)
                .Include(v => v.medioPago)
                .ToListAsync();
            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var venta = await _dbContext.Ventas
                    .Include(v => v.cliente)
                    .Include(v => v.medioPago)
                    .Include(v => v.detalles).ThenInclude(d => d.Producto)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.idVenta == id);

                if(venta == null)
                {
                    TempData["Error"] = "Venta no Encontrada";
                    Console.WriteLine("Error al cargar la Venta");
                    return RedirectToAction(nameof(Lista));
                }
                return View(venta);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles de la Venta";
                Console.WriteLine($"Erorr al cargar los detalles: {ex.Message}" );
                return RedirectToAction(nameof(Lista));
            
            }
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            try
            {
                //Cargar datos necesarios para los dropdowns
                var clientes = await _dbContext.Clientes.AsNoTracking().ToListAsync();
                var mediopago = await _dbContext.MedioPagos.AsNoTracking().ToListAsync();
                var productos = await _dbContext.Productos.AsNoTracking().ToListAsync();

                // Validar que existan datos necesarios
                if (!clientes.Any())
                {
                    Console.WriteLine("Error al cargar cliente");
                    TempData["Error"] = "Debe existir al menos un cliente, medio de pago y producto activo";
                    return RedirectToAction(nameof(Lista));
                }
                if (!mediopago.Any())
                {
                    Console.WriteLine("Error al cargar medio de pago");
                    TempData["Error"] = "Debe existir al menos un cliente, medio de pago y producto activo";
                    return RedirectToAction(nameof(Lista));
                }
                if (!productos.Any())
                {
                    Console.WriteLine("Error al cargar productos");
                    TempData["Error"] = "Debe existir al menos un cliente, medio de pago y producto activo";

                    return RedirectToAction(nameof(Lista));
                }
                // Preparar ViewBags para los selects
                ViewBag.Productos = productos;
                ViewBag.Clientes = new SelectList(await _dbContext.Clientes.AsNoTracking().ToListAsync(), "idCliente", "nombreCliente");
                ViewBag.MediosPago = new SelectList(await _dbContext.MedioPagos.AsNoTracking().ToListAsync(), "idMedioPago", "nombre");
                // Inicializar modelo con fecha actual
                var model = new VentaVM
                {
                    fecha_registro = DateOnly.FromDateTime(DateTime.Now),
                    Detalles = new List<DetalleVM>() { new DetalleVM() }
                };
                return View(model);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar Nueva Venta: {ex.Message}");
                TempData["Error"] = "Error al cargar el formulario de venta";
                return RedirectToAction(nameof(Lista));
            }
        }






        [HttpGet]
        public IActionResult ObtenerFilaDetalle()
        {
            var productos = _dbContext.Productos.AsNoTracking().ToList();
            ViewBag.Productos = productos;
            return PartialView("DetalleVentaRow", new DetalleVM());
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
        public async Task<IActionResult> Nuevo (VentaVM model)
        {
            try
            {
                // Limpiar ModelState para propiedades de navegación
                ModelState.Remove("Detalles");

                if (ModelState.IsValid && model.Detalles != null && model.Detalles.Any())
                {
                    using (var transaccion = await _dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var clienteExist = await _dbContext.Clientes.AnyAsync(c => c.idCliente == model.idCliente);
                            var medioPagoExist = await _dbContext.MedioPagos.AnyAsync(m => m.idMedioPago == model.idMedioPago);
                            if (!clienteExist || !medioPagoExist)
                            {
                                ModelState.AddModelError("", "Cliente o medio de pago no válido");
                                Console.Write("Error al cargar los ID de Medio de Pago o Cliente ");
                                throw new Exception("Datos referenciales inválidos");
                            }

                            var venta = new Venta
                            {
                                fecha_registro = model.fecha_registro,
                                idCliente = model.idCliente,
                                idMedioPago = model.idMedioPago,
                                total = model.Detalles.Sum(d => d.subtotal),
                                estado = Venta.estadoventa.Pendiente,
                                detalles = new List<DetalleVenta>()
                            };
                            foreach (var detallevm in model.Detalles)
                            {
                                var producto = await _dbContext.Productos.FindAsync(detallevm.idProducto);
                                if (producto == null)
                                {
                                    throw new Exception($"Produto con ID {detallevm.idProducto} no encontrado");
                                }
                                ;
                                var detalle = new DetalleVenta
                                {
                                    idProducto = detallevm.idProducto,
                                    cantidad = detallevm.cantidad,
                                    precio_unitario = detallevm.precio_unitario,
                                    subtotal = detallevm.subtotal,
                                };

                                venta.detalles.Add(detalle);

                                //actualizar stock
                                producto.stock -= detallevm.cantidad;
                                //_dbContext.Productos.Add(producto);
                            }

                            _dbContext.Ventas.Add(venta);
                            await _dbContext.SaveChangesAsync();
                            await transaccion.CommitAsync();

                            Console.WriteLine("Exito al registrar una Venta");
                            TempData["Exito"] = "Venta registrada correctamente";
                            return RedirectToAction(nameof(Lista));
                        }
                        catch (Exception ex)
                        {
                            await transaccion.RollbackAsync();
                            ModelState.AddModelError("", "Error al registrar la venta: " + ex.Message);
                            Console.WriteLine($"Error en transacción: {ex.Message}");
                        }
                    }
                }
                else if (model.Detalles == null || !model.Detalles.Any()) 
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la venta");
                    Console.WriteLine("Error al cargar los productos a la venta");
                }
                // Recargar datos necesarios si hay error
                ViewBag.Clientes = new SelectList(await _dbContext.Clientes.AsNoTracking().ToListAsync(), "idCliente", "nombre");
                ViewBag.MediosPago = new SelectList(await _dbContext.MedioPagos.AsNoTracking().ToListAsync(), "idMedioPago", "descripcion");
                ViewBag.Productos = new SelectList(await _dbContext.Productos.AsNoTracking().ToListAsync(), "idProducto", "nombreProducto");

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Venta/Nuevo(POST): {ex.Message}");
                TempData["Error"] = "Error al procesar la venta";
                return RedirectToAction(nameof(Lista));
            }

        }

    }
}
