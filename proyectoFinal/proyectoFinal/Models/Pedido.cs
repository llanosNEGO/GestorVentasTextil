using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace proyectoFinal.Models
{
    public class Pedido
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idPedido {  get; set; }
        [Required]
        public DateOnly fechaPedido { get; set; }
        [Required]
        public DateTime fechaEntrega { get; set; }
        [Required, StringLength(50)]
        public string direccionEntrega { get; set; }
        [Required, Precision(10,2)]
        public decimal subtotal { get; set; }
        public enum estadoPedido
        {
            Pendiente,
            EnProceso,
            Enviado,
            Entregado,
            Cancelado
        }

        [Required]
        public estadoPedido EstadoPedido { get; set; } = estadoPedido.Pendiente;
        [StringLength(500), Required]
        public string? Comentario { get; set; }

        //FK
        [ForeignKey("Cliente")]
        public int? idcliente { get; set; }
        public Cliente? cliente { get; set; }

        public ICollection<Venta> ventas { get; set; }

        public ICollection<DetallePedido> detalles { get; set; }
    }
}
