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
        [Required]
        public bool estado {  get; set; }

        //FK

        [ForeignKey("DetalleVenta")]
        public DetalleVenta detalleVenta { get; set; }
        public int idDetalle { get; set; }

        [ForeignKey("Cliente")]
        public Cliente cliente { get; set; }
        public int idCliente { get; set; }

        [ForeignKey("MedioPago")]
        public MedioPago medioPago { get; set; }
        public int idMedioPago { get; set; }

        [ForeignKey("Usuario")]
        public Usuario usuario { get; set; }
        public int idUsuario { get; set; }
    }
}
