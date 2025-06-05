using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class Producto
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProducto { get; set; }
        [StringLength(50),Required]
        public string nombreProducto { get; set; }
        [StringLength(50),Required]
        public string descripcionProducto { get; set; }
        [Required]
        public decimal precioProducto { get; set; }

        //FK

        [ForeignKey("Categoria")]
        public Categoria categoria { get; set; }
        public int idCategoria { get; set; }

        public ICollection<Inventario> inventario { get; set; }
        public ICollection<DetalleCompra> detalleCompras { get; set; }
        public ICollection<DetalleVenta> detalleVentas { get; set; }
    }
}
