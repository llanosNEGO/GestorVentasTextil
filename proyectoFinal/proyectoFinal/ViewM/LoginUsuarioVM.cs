using System.ComponentModel.DataAnnotations;

namespace proyectoFinal.ViewM
{
    public class LoginUsuarioVM
    {
        public string nombreUsuario { get; set; }
        public string password { get; set; }
        public string confirmarpassword { get; set; }
    }
}
