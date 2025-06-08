using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class MedioPago
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idMedioPago { get; set; }
        [StringLength(50),Required]
        public string nombre {  get; set; }
        //FK
        public ICollection<Venta> ventas { get; set; }
        public ICollection<ComprasProveedor> comprasProveedores { get; set; }

    }
}
