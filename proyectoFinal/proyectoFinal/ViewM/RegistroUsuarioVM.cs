namespace proyectoFinal.ViewM
{
    public class RegistroUsuarioVM
    {
        public string nombreUsuario { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string confirmarpassword { get; set; }
        public DateTime fecha_registro { get; set; } = DateTime.Now;
        public int idRol {  get; set; }
    }
}
