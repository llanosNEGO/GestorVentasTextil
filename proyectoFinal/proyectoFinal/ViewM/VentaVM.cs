using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using proyectoFinal.ViewM;

namespace proyectoFinal.ViewM
{
    public class VentaVM
    {

        public DateOnly fecha_registro { get; set; }

        public int idCliente { get; set; }
        public int idMedioPago { get; set; }



        public List<DetalleVM> Detalles { get; set; } = new List<DetalleVM>();
    }
    public class DetalleVM
    {
        public int idProducto { get; set; }

        public int cantidad { get; set; }

        public decimal precio_unitario { get; set; }

        public decimal subtotal => cantidad * precio_unitario;
    }
}
