using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoFinal.Models
{
    public class Proveedor
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idProveedor { get; set; }
        [StringLength(50) , Required]
        public string nombreEmpresa { get; set; }

        //FK
        public ICollection<ComprasProveedor> comprasProveedors { get; set; }
    }
}
