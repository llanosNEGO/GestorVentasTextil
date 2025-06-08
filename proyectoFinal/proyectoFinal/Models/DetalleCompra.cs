using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class DetalleCompra
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleCompra { get; set; }
        [Required]
        public decimal precioUnitario { get; set; }
        [Required]
        public int cantidad {  get; set; }
        [StringLength(50) , Required]
        public string color { get; set; }
        [StringLength(50) , Required]
        public string talla { get; set; }

        [Required]
        public decimal subtotal { get; set; }

        //FK
        [ForeignKey("idCompras")]
        public ComprasProveedor ComprasProveedor { get; set; }
        public int idCompras { get; set; }

        [ForeignKey("Producto")]
        public Producto productos { get; set; }
        public int idProducto { get; set; }

    }
}
