using proyectoFinal.Data;
using proyectoFinal.Models;
using proyectoFinal.ViewM;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;
using proyectoFinal.ViewM;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace proyectoFinal.Controllers
{
    [Authorize(Roles = "Administrador")]
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
        public async Task<IActionResult> Nuevo(int? idPedido = null)
        {
            try
            {
                // Cargar datos necesarios para los dropdowns
                var clientes = await _dbContext.Clientes.AsNoTracking().ToListAsync();
                var mediopago = await _dbContext.MedioPagos.AsNoTracking().ToListAsync();
                var productos = await _dbContext.Productos.AsNoTracking().ToListAsync();

                if (idPedido.HasValue && idPedido <= 0)
                {
                    TempData["Error"] = "ID de pedido inválido";
                    return RedirectToAction(nameof(Lista));
                }

                // Validar que existan datos necesarios
                if (!clientes.Any() || !mediopago.Any() || !productos.Any())
                {
                    Console.WriteLine("Debe existir al menos un cliente, medio de pago y producto activo");
                    TempData["Error"] = "Debe existir al menos un cliente, medio de pago y producto activo";
                    return RedirectToAction(nameof(Lista));
                }

                // Cargar pedidos disponibles (solo aprobados sin venta asociada)
                var pedidosDisponibles = await _dbContext.Pedidos
                    .Include(p => p.cliente)
                    .Where(p => p.EstadoPedido == Pedido.estadoPedido.Pendiente &&
                                !p.ventas.Any() &&
                                p.idPedido > 0) 
                    .OrderByDescending(p => p.idPedido) // Ordenar por ID descendente
                    .Select(p => new {
                        p.idPedido,
                        DisplayText = $"Pedido #{p.idPedido} - {p.cliente.nombreCliente} - {p.fechaPedido.ToShortDateString()}"
                    })
                    .ToListAsync();

                // Preparar ViewBags para los selects
                ViewBag.Productos = productos;
                ViewBag.Clientes = new SelectList(clientes, "idCliente", "nombreCliente");
                ViewBag.MediosPago = new SelectList(mediopago, "idMedioPago", "nombre");
                ViewBag.PedidosDisponibles = new SelectList(pedidosDisponibles, "idPedido", "DisplayText");

                // Inicializar modelo con fecha actual
                var model = new VentaVM
                {
                    fecha_registro = DateTime.Now,
                    Detalles = new List<DetalleVM>() { new DetalleVM() },
                    PedidosDisponibles = new SelectList(pedidosDisponibles, "idPedido", "DisplayText")
                };

                // Si se proporcionó un idPedido, cargar sus datos
                if (idPedido.HasValue)
                {
                    await CargarDatosPedido(model, idPedido.Value);
                }
                else
                {
                    // Agregar una fila vacía por defecto
                    model.Detalles.Add(new DetalleVM());
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar Nueva Venta: {ex.Message}");
                TempData["Error"] = "Error al cargar el formulario de venta";
                return RedirectToAction(nameof(Lista));
            }
        }

        private async Task CargarDatosPedido(VentaVM model, int idPedido)
        {
            var pedido = await _dbContext.Pedidos
                .Include(p => p.cliente)
                .Include(p => p.detalles)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(p => p.idPedido == idPedido);

            if (pedido != null)
            {
                model.SelectedPedidoId = pedido.idPedido;
                model.idCliente = pedido.idcliente ?? 0;
                model.PedidoClienteNombre = pedido.cliente?.nombreCliente;
                model.PedidoSubtotal = pedido.subtotal;
                model.PedidoFecha = pedido.fechaPedido;
                model.PedidoDireccion = pedido.direccionEntrega;

                // Mapear detalles del pedido a detalles de venta
                model.Detalles = pedido.detalles.Select(d => new DetalleVM
                {
                    idProducto = d.idproducto,
                    nombreProducto = d.producto?.nombreProducto,
                    cantidad = d.cantidad,
                    precio_unitario = d.precio_unitario,
                    // subtotal se calcula automáticamente
                }).ToList();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPedidoInfo(int id)
        {
            // Validación reforzada
            if (id <= 0)
            {
                Console.WriteLine($"ID de pedido inválido recibido: {id}");
                return Json(new
                {
                    success = false,
                    message = $"ID de pedido inválido recibido: {id}"
                });
            }

            try
            {
                // Consulta optimizada
                var pedido = await _dbContext.Pedidos
                    .AsNoTracking()
                    .Include(p => p.cliente)
                    .Include(p => p.detalles)
                        .ThenInclude(d => d.producto)
                    .Where(p => p.idPedido == id && p.EstadoPedido == Pedido.estadoPedido.Pendiente)
                    .Select(p => new {
                        p.idPedido,
                        p.idcliente,
                        clienteNombre = p.cliente.nombreCliente,
                        p.fechaPedido,
                        p.direccionEntrega,
                        p.subtotal,
                        detalles = p.detalles.Select(d => new {
                            d.idproducto,
                            nombreProducto = d.producto.nombreProducto,
                            d.cantidad,
                            d.precio_unitario,
                            d.subtotal
                        })
                    })
                    .FirstOrDefaultAsync();

                if (pedido == null)
                {
                    Console.WriteLine($"Pedido {id} no encontrado o no está pendiente");
                    return Json(new
                    {
                        success = false,
                        message = "Pedido no encontrado o no está pendiente"
                    });
                }

                Console.WriteLine($"Pedido {id} cargado correctamente");

                return Json(new
                {
                    success = true,
                    idPedido = pedido.idPedido,
                    idCliente = pedido.idcliente,
                    clienteNombre = pedido.clienteNombre,
                    subtotal = pedido.subtotal,
                    fechaPedido = pedido.fechaPedido.ToString("yyyy-MM-dd"),
                    direccionEntrega = pedido.direccionEntrega,
                    detalles = pedido.detalles
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex} : Error al obtener pedido {id}");
                return Json(new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
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
                ModelState.Remove("PedidosDisponibles");

                if (model.SelectedPedidoId.HasValue)
                {
                    var pedido = await _dbContext.Pedidos.
                        Include(p => p.ventas)
                        .FirstOrDefaultAsync(p => p.idPedido == model.SelectedPedidoId);
                    if (pedido == null)
                    {
                        ModelState.AddModelError("SelectedPedidoId", "El pedido seleccionado no existe");
                        Console.WriteLine("Error al cargar el Pedido seleccionado");
                    }
                    else if (pedido.ventas.Any())
                    {
                        ModelState.AddModelError("SelectedPedidoId", "Este pedido ya tiene una venta asociada");
                    }
                }


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
                                };

                                if(producto.stock < detallevm.cantidad)
                                {
                                    throw new Exception($"Stock insuficiente para el producto {producto.nombreProducto}");
                                };

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

                var pedidosDisponibles = await _dbContext.Pedidos
                    .Include(p => p.cliente)
                    .Where( p => p.EstadoPedido == Pedido.estadoPedido.Pendiente && !p.ventas.Any())
                    .Select(p => new {
                         p.idPedido,
                         DisplayText = $"Pedido #{p.idPedido} - {p.cliente.nombreCliente} - {p.fechaPedido.ToShortDateString()}"
                     })
                    .ToListAsync();
                model.PedidosDisponibles = new SelectList(pedidosDisponibles, "idPedido", "DisplayText");

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
