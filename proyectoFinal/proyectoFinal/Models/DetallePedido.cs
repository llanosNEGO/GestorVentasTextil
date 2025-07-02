using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace proyectoFinal.Models
{
    public class DetallePedido
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetallePedido {  get; set; }
        [Required]
        public int cantidad { get; set; }
        [Required, Precision(10,2)]
        public decimal precio_unitario { get; set; }
        [Required, Precision(10,2)]
        public decimal subtotal { get; set; }

        //FK 

        [ForeignKey("Pedido")]
        public int idpedido { get; set; }
        public Pedido pedido {  get; set; }

        [ForeignKey("Producto")]
        public int idproducto { get; set; }
        public Producto producto { get; set; }

    }
}
