using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idUsuario { get; set; }
        [StringLength(50),Required]
        public string nombreUsuario { get; set; }
        [StringLength(50),Required]
        public string email {  get; set; }
        [StringLength(50),Required]
        public string password { get; set; }
        [Required]
        public DateOnly fecha_registro { get; set; }

        //FK

        [ForeignKey("Rol")]
        public Rol rol {  get; set; }
        public int idRol { get; set; }

        public ICollection<Venta> ventas { get; set; }
    }
}
