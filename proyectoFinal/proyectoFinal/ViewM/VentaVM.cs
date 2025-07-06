using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using proyectoFinal.ViewM;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.ViewM
{
    public class VentaVM
    {

        public DateTime fecha_registro { get; set; }

        public int idCliente { get; set; }
        public int idMedioPago { get; set; }


        [Display(Name = "Pedido Asociado")]
        [JsonIgnore] 
        public int? SelectedPedidoId { get; set; }

        public SelectList PedidosDisponibles { get; set; } = new SelectList(new List<SelectListItem>());

        public string clienteNombre { get; set; }
        public decimal? PedidoSubtotal { get; set; }
        public DateOnly? PedidoFecha { get; set; }
        public string direccionEntrega { get; set; }

        public List<DetalleVM> Detalles { get; set; } = new List<DetalleVM>();
    }
    public class DetalleVM
    {
        public int idProducto { get; set; }

        public string nombreProducto { get; set; }
        public int cantidad { get; set; }

        public int precio_unitario { get; set; }

        public int subtotal => cantidad * precio_unitario;
    }
}
