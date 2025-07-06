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
                

                if (idPedido.HasValue && idPedido <= 0)
                {
                    TempData["Error"] = "ID de pedido inválido";
                    return RedirectToAction(nameof(Lista));
                }


                // Inicializar modelo con fecha actual
                var model = new VentaVM
                {
                    fecha_registro = DateTime.Now,
                    Detalles = new List<DetalleVM>() { new DetalleVM() },
                };
                await InicializarViewBags(model, idPedido);

                var clientes = ViewBag.Clientes as SelectList;
                var mediosPago = ViewBag.MediosPago as SelectList;

                if (clientes?.Count() == 0 || mediosPago?.Count() == 0)
                {
                    TempData["Error"] = "Debe existir al menos un cliente y medio de pago";
                    return RedirectToAction(nameof(Lista));
                }

                // Si no hay pedido seleccionado, validar productos
                if (!idPedido.HasValue)
                {
                    var productos = ViewBag.Productos as List<Producto>;
                    if (productos?.Count == 0)
                    {
                        TempData["Error"] = "No hay productos disponibles";
                        return RedirectToAction(nameof(Lista));
                    }
                }
                if (idPedido.HasValue)
                {
                    await CargarDatosPedido(model, idPedido.Value);
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
            if (idPedido <= 0)
            {
                Console.WriteLine($"ID de pedido inválido en CargarDatosPedido: {idPedido}");
                throw new ArgumentException("ID de pedido inválido");
            }
            var pedido = await _dbContext.Pedidos
                .Include(p => p.cliente)
                .Include(p => p.detalles)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(p => p.idPedido == idPedido);


            if (pedido == null)
            {
                Console.WriteLine($"Pedido {idPedido} no encontrado o no está pendiente");
                throw new Exception($"Pedido {idPedido} no encontrado o no está pendiente");
            }
            if (pedido != null)
            {
                model.SelectedPedidoId = pedido.idPedido;
                model.idCliente = pedido.idcliente ?? 0;
                model.clienteNombre = pedido.cliente?.nombreCliente;
                model.PedidoSubtotal = pedido.subtotal;
                model.PedidoFecha = pedido.fechaPedido;
                model.direccionEntrega = pedido.direccionEntrega;

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
                ModelState.Remove("clienteNombre");
                ModelState.Remove("direccionEntrega");
                ModelState.Remove("PedidoSubtotal");
                ModelState.Remove("PedidoFecha");

                if (model.Detalles != null)
                {
                    for (int i = 0; i < model.Detalles.Count; i++)
                    {
                        ModelState.Remove($"Detalles[{i}].nombreProducto");
                        ModelState.Remove($"Detalles[{i}].idProducto");
                        ModelState.Remove($"Detalles[{i}].cantidad");
                        ModelState.Remove($"Detalles[{i}].precio_unitario");
                    }
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                    Console.WriteLine($"Errores de validación: {string.Join(", ", errors)}");
                    await InicializarViewBags(model, model.SelectedPedidoId);
                    return View(model);
                }
                if (model.Detalles == null || !model.Detalles.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la venta");
                    Console.WriteLine("Intento de crear venta sin detalles");
                    await InicializarViewBags(model, model.SelectedPedidoId);
                    return View(model);
                }

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
                                estado = Venta.estadoventa.Completada,
                                detalles = new List<DetalleVenta>()
                            };
                            if (model.SelectedPedidoId.HasValue)
                            {
                                var pedido = await _dbContext.Pedidos
                                    .FirstOrDefaultAsync(p => p.idPedido == model.SelectedPedidoId.Value);

                                if (pedido != null)
                                {
                                    pedido.EstadoPedido = Pedido.estadoPedido.Enviado;
                                    _dbContext.Pedidos.Update(pedido);

                                    // Asociar la venta con el pedido
                                    venta.idpedido = pedido.idPedido;
                                }
                            }

                            foreach (var detallevm in model.Detalles)
                            {
                                var producto = await _dbContext.Productos.FindAsync(detallevm.idProducto);
                                if (producto == null)
                                {
                                    throw new Exception($"Producto con ID {detallevm.idProducto} no encontrado");
                                }

                                if (producto.stock < detallevm.cantidad)
                                {
                                    throw new Exception($"Stock insuficiente para el producto {producto.nombreProducto}");
                                }

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
                await InicializarViewBags(model, model.SelectedPedidoId);

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Venta/Nuevo(POST): {ex.Message}");
                TempData["Error"] = "Error al procesar la venta";
                return RedirectToAction(nameof(Lista));
            }

        }
        private async Task InicializarViewBags(VentaVM model, int? idPedido = null)
        {
            try
            {
                // Cargar datos necesarios para los dropdowns
                var clientes = await _dbContext.Clientes.AsNoTracking().ToListAsync();
                var mediopago = await _dbContext.MedioPagos.AsNoTracking().ToListAsync();

                // Cargar productos solo si no hay pedido seleccionado
                List<Producto> productos = new List<Producto>();
                if (!idPedido.HasValue)
                {
                    productos = await _dbContext.Productos.AsNoTracking().ToListAsync();
                }

                // Cargar pedidos disponibles
                var pedidosDisponibles = await _dbContext.Pedidos
                    .Include(p => p.cliente)
                    .Where(p => p.EstadoPedido == Pedido.estadoPedido.Pendiente &&
                                !p.ventas.Any() &&
                                p.idPedido > 0)
                    .OrderByDescending(p => p.idPedido)
                    .Select(p => new {
                        p.idPedido,
                        DisplayText = $"Pedido #{p.idPedido} - {p.cliente.nombreCliente} - {p.fechaPedido.ToShortDateString()}"
                    })
                    .ToListAsync();

                // Configurar ViewBags de forma consistente
                ViewBag.Productos = productos; // Mantener como List<Producto>
                ViewBag.Clientes = new SelectList(clientes, "idCliente", "nombreCliente"); // Usar nombreCliente
                ViewBag.MediosPago = new SelectList(mediopago, "idMedioPago", "nombre"); // Usar nombre
                ViewBag.PedidosDisponibles = new SelectList(pedidosDisponibles, "idPedido", "DisplayText");
                ViewBag.TienePedidoSeleccionado = idPedido.HasValue;

                // Actualizar el modelo
                model.PedidosDisponibles = new SelectList(pedidosDisponibles, "idPedido", "DisplayText");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al inicializar ViewBags: {ex.Message}");
                // Inicializar con listas vacías para evitar nulls
                ViewBag.Productos = new List<Producto>();
                ViewBag.Clientes = new SelectList(new List<Cliente>(), "idCliente", "nombreCliente");
                ViewBag.MediosPago = new SelectList(new List<MedioPago>(), "idMedioPago", "nombre");
                ViewBag.PedidosDisponibles = new SelectList(new List<object>(), "idPedido", "DisplayText");
                ViewBag.TienePedidoSeleccionado = false;
                model.PedidosDisponibles = new SelectList(new List<object>(), "idPedido", "DisplayText");
            }
        }

    }
}
