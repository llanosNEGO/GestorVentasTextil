using System.ComponentModel.DataAnnotations;

namespace proyectoFinal.ViewM
{
    public class LoginUsuarioVM
    {
        
        public string email { get; set; }

        public string password { get; set; }
        [Display(Name = "Recordar sesión")]
        public bool recordar { get; set; }
    }
}
