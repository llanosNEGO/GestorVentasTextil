using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace proyectoFinal.Models
{
    public class Venta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idVenta { get; set; }
        [Required]
        public DateOnly fecha_registro { get; set; }
        [Required, Precision(5,2)]
        public decimal total {  get; set; }
        public enum estadoventa
        {
            Pendiente,
            Completada,
            Cancelada
        }

        [Required]
        public estadoventa estado { get; set; } = estadoventa.Pendiente;

        //FK

        public ICollection<DetalleVenta> detalles { get; set; }

        [ForeignKey("Pedido")]
        public Pedido? pedido { get; set; }
        public int? idpedido { get; set; }


        [ForeignKey("Cliente")]
        public Cliente cliente { get; set; }
        public int idCliente { get; set; }

        [ForeignKey("MedioPago")]
        public MedioPago medioPago { get; set; }
        public int idMedioPago { get; set; }

    }
}
