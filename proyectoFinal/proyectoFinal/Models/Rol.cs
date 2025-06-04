using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class Rol
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idRol {  get; set; }
        [StringLength(50),Required]
        public string nombreRol { get; set; }
        [StringLength(50),Required]
        public string descripcion { get; set; }

        public ICollection<Usuario> usuarios { get; set; }
    }
}
