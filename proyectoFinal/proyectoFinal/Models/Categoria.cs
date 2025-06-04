using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class Categoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCategoria { get; set; }
        [StringLength(50) , Required]
        public string nombreCategoria { get; set; }
        [StringLength(50) , Required]
        public string descripcion { get; set; }

        //FK

        public ICollection<Producto> Productos { get; set; }
    }
}
