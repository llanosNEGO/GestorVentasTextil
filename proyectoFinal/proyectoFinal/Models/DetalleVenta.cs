using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace proyectoFinal.Models
{
    public class DetalleVenta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalle { get; set; }
        [Required]
        public int cantidad { get; set; }
        [Required, Precision(5,2)]
        public decimal precio_unitario { get; set; }
        [Required, Precision(5,2)]
        public decimal subtotal { get; set; }

        //FK
        public ICollection<Venta> ventas { get; set; }

        [ForeignKey("Produto")]
        public Producto Producto { get; set; }
        public int idProducto { get; set; }
    }
}
