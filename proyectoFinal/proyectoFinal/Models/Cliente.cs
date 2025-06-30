using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class Cliente
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCliente { get; set; }
        [StringLength(50),Required]
        public string nombreCliente { get; set; }
        [Required]
        public int telefono { get; set; }
        [StringLength(50), Required]
        public string email { get; set; }

        //FK

        public ICollection<Venta> ventas { get; set; }
        public ICollection<Pedido> pedidos { get; set; }
    }
}
