using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class Producto
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProducto { get; set; }
        [StringLength(50),Required]
        public string nombreProducto { get; set; }
        [StringLength(50),Required]
        public string descripcionProducto { get; set; }
        [Required]
        public decimal precioProducto { get; set; }
        [Required]
        public int stock { get; set; }

        [StringLength (255)]
        public string ImagenUrl { get; set; } = "https://static.vecteezy.com/system/resources/previews/004/141/669/large_2x/no-photo-or-blank-image-icon-loading-images-or-missing-image-mark-image-not-available-or-image-coming-soon-sign-simple-nature-silhouette-in-frame-isolated-illustration-vector.jpg"; // Default image URL

        //FK

        [ForeignKey("Categoria")]
        public Categoria categoria { get; set; }
        public int idCategoria { get; set; }

        public ICollection<Inventario> inventario { get; set; }
        public ICollection<DetalleCompra> detalleCompras { get; set; }
        public ICollection<DetalleVenta> detalleVentas { get; set; }
    }
}
