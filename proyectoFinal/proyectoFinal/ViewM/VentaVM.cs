using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using proyectoFinal.ViewM;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http.HttpResults;

namespace proyectoFinal.ViewM
{
    public class VentaVM
    {

        public DateTime fecha_registro { get; set; }

        public int idCliente { get; set; }
        public int idMedioPago { get; set; }


        [Display(Name = "Pedido Asociado")]
        public int? SelectedPedidoId { get; set; }

        public SelectList PedidosDisponibles { get; set; } = new SelectList(new List<SelectListItem>());

        public string PedidoClienteNombre { get; set; }
        public decimal? PedidoSubtotal { get; set; }
        public DateOnly? PedidoFecha { get; set; }
        public string PedidoDireccion { get; set; }

        public List<DetalleVM> Detalles { get; set; } = new List<DetalleVM>();
    }
    public class DetalleVM
    {
        public int idProducto { get; set; }

        public string nombreProducto { get; set; }
        public int cantidad { get; set; }

        public decimal precio_unitario { get; set; }

        public decimal subtotal => cantidad * precio_unitario;
    }
}
