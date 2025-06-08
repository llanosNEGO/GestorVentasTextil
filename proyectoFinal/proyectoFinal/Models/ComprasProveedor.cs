using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace proyectoFinal.Models
{
    public class ComprasProveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCompraProveedor { get; set; }
        [Required]
        public DateOnly fecha_registro { get; set; }
        [Required , Precision(5,2)]
        public decimal total {  get; set; }
        [Required]
        public bool estado {  get; set; }

        //FK

        [ForeignKey("Proveedor")]
        public Proveedor proveedor { get; set; }
        public int idProveedor { get; set; }

        [ForeignKey("MedioPago")]
        public MedioPago medioPago { get; set; }
        public int idmedioPago { get; set; }

        public ICollection<DetalleCompra> detalleCompras { get; set; }

    }
}
