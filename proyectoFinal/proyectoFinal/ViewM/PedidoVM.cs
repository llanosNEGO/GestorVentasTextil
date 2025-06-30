using System.ComponentModel.DataAnnotations;

namespace proyectoFinal.ViewM
{
    public class PedidoVM
    {
        public DateOnly fechaPedido { get; set; }

        public DateTime fechaEntrega { get; set; }

        public string direccionEntrega { get; set; }

        public decimal subtotal { get; set; }

        public int idCliente { get; set; }

        public string? Comentario { get; set; }

        public List<DetallePedidoVM> Detalles { get; set; } = new List<DetallePedidoVM>();
    }

    public class DetallePedidoVM
    {

        public int idProducto { get; set; }

        public int cantidad { get; set; }

        public decimal precioUnitario { get; set; }

        public decimal subtotal => cantidad * precioUnitario;
    }
}

