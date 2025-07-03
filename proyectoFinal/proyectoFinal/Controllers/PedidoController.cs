using Microsoft.EntityFrameworkCore;
using proyectoFinal.Models;
using proyectoFinal.Data;
using proyectoFinal.ViewM;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using proyectoFinal.ViewM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace proyectoFinal.Controllers
{
    [Authorize(Roles = "Administrador, Usuario")]
    public class PedidoController : Controller
    {
        private readonly AppDBContext _dbContext;

        public PedidoController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<Pedido> lista = await _dbContext.Pedidos
                .Include( p => p.cliente)
                .ToListAsync();
            return View(lista);
        }
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var pedido = await _dbContext.Pedidos
                    .Include( p => p.cliente)
                    .Include( p => p.detalles).ThenInclude( p => p.producto).AsNoTracking()
                    .FirstOrDefaultAsync(p => p.idPedido == id);

                if (pedido == null) {
                    ViewData["Mensaje"] = " Pedido no encontrado";
                    Console.WriteLine("Error al cargar el detalle de pedido, error en id de pedido");
                    return RedirectToAction(nameof(Lista));
                }
                return View(pedido);
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "Error al cargar los detalles de la venta ";
                Console.WriteLine($"Error al cargar pedido error: {ex.Message}");
                return RedirectToAction(nameof(Lista));
            }
        }
        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            try
            {
                var cliente = await _dbContext.Clientes.AsNoTracking().ToListAsync();
                var productos = await _dbContext.Productos.AsNoTracking().ToListAsync();

                if (cliente == null)
                {
                    Console.WriteLine("Error al cargar clientes");
                    TempData["Error"] = "Debe existir al menos un cliente y producto activo";
                    return RedirectToAction(nameof(Lista));
                }
                if (productos == null) {

                    Console.WriteLine("Error al cargar productos");
                    TempData["Error"] = "Debe existir al menos un cliente y producto activo";
                    return RedirectToAction(nameof(Lista));
                }

                ViewBag.Productos = productos;
                ViewBag.Clientes = new SelectList(await _dbContext.Clientes.AsNoTracking().ToListAsync(), "idCliente", "nombreCliente");
                // Inicializar modelo con fecha actual
                var model = new PedidoVM
                {
                    fechaPedido = DateOnly.FromDateTime(DateTime.Now),
                    fechaEntrega = DateTime.Now.AddDays(3), // Entrega en 3 días por defecto
                    Detalles = new List<DetallePedidoVM>() { new DetallePedidoVM() }
                };
                return View(model);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error al cargar Nuevo Pedido: {e.Message}");
                TempData["Error"] = "Error al cargar el formulario de pedido";
                return RedirectToAction(nameof(Lista));
            }

        }

        [HttpGet]
        public IActionResult ObtenerFilaDetalle()
        {
            var productos = _dbContext.Productos.AsNoTracking().ToList();
            ViewBag.Productos = productos;
            return PartialView("DetallePedidoRow", new DetallePedidoVM());
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
        public async Task<IActionResult> Nuevo(PedidoVM model)
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
                            if (!clienteExist)
                            {
                                ModelState.AddModelError("", "Cliente no válido");
                                Console.Write("Error al cargar el ID de Cliente");
                                throw new Exception("Cliente inválido");
                            }

                            var pedido = new Pedido
                            {
                                fechaPedido = model.fechaPedido,
                                fechaEntrega = model.fechaEntrega,
                                direccionEntrega = model.direccionEntrega,
                                subtotal = model.Detalles.Sum(d => d.subtotal),
                                EstadoPedido = Pedido.estadoPedido.Pendiente, // Estado inicial
                                Comentario = model.Comentario,
                                idcliente = model.idCliente,
                                detalles = new List<DetallePedido>()
                            };

                            foreach (var detallevm in model.Detalles)
                            {
                                var producto = await _dbContext.Productos.FindAsync(detallevm.idProducto);
                                if (producto == null)
                                {
                                    throw new Exception($"Producto con ID {detallevm.idProducto} no encontrado");
                                }

                                var detalle = new DetallePedido
                                {
                                    idproducto = detallevm.idProducto,
                                    cantidad = detallevm.cantidad,
                                    precio_unitario = detallevm.precioUnitario,
                                    subtotal = detallevm.subtotal,
                                };

                                pedido.detalles.Add(detalle);
                            }

                            _dbContext.Pedidos.Add(pedido);
                            await _dbContext.SaveChangesAsync();
                            await transaccion.CommitAsync();

                            Console.WriteLine("Éxito al registrar un Pedido");
                            TempData["Exito"] = "Pedido registrado correctamente";
                            return RedirectToAction(nameof(Lista));
                        }
                        catch (Exception ex)
                        {
                            await transaccion.RollbackAsync();
                            ModelState.AddModelError("", "Error al registrar el pedido: " + ex.Message);
                            Console.WriteLine($"Error en transacción: {ex.Message}");
                        }
                    }
                }
                else if (model.Detalles == null || !model.Detalles.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto al pedido");
                    Console.WriteLine("Error al cargar los productos al pedido");
                }

                // Recargar datos necesarios si hay error
                ViewBag.Clientes = new SelectList(await _dbContext.Clientes.AsNoTracking().ToListAsync(), "idCliente", "nombreCliente");
                ViewBag.Productos = new SelectList(await _dbContext.Productos.AsNoTracking().ToListAsync(), "idProducto", "nombreProducto");

                return View(model);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al cargar pedido en POST : {e.Message}");
                TempData["Error"] = " Error al procesar el Pedido";
                return RedirectToAction(nameof(Lista));
            }
        }

    }
}
