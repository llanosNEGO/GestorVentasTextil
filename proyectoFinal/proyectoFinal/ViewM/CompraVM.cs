using System.Xml.Linq;

namespace proyectoFinal.ViewM
{
    public class CompraVM
    {
        public DateOnly fecha_registro {  get; set; }
        public int idProveedor { get; set; }
        public int idmedioPado { get; set; }

        public List<DetalleCompraVM> detalles { get; set; } = new List<DetalleCompraVM>();
    }

    public class DetalleCompraVM
    {
        public int idProducto { get; set; }
        public int cantidad { get; set; }
        public string color {  get; set; }
        public string talla { get; set; }
        public int precioUnitario { get; set; }

        public int subtotal => cantidad * precioUnitario;

    }
}
