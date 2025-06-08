using System.ComponentModel.DataAnnotations;

namespace proyectoFinal.ViewM
{
    public class LoginUsuarioVM
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string nombreUsuario { get; set; }
        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }
}
