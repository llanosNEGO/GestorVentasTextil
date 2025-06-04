using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class Inventario
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idInventario { get; set; }
        [StringLength(50), Required]
        public string talla {  get; set; }
        [StringLength(50), Required]
        public string color { get; set; }
        [Required]
        public int Cantidad { get; set; }

        //FK
        [ForeignKey("Producto")]
        public Producto producto { get; set; }
        public int idProducto { get; set; }
    }
}
