using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoFinal.Data;
using proyectoFinal.Models;
using proyectoFinal.ViewM;
using System.Security.Claims;

namespace proyectoFinal.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly AppDBContext _dbContext;
        

        public LoginController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
            InitializeRoles().Wait();
            InitialzeUsuario().Wait();
        }

        private async Task InitializeRoles()
        {
            if (!await _dbContext.Rol.AnyAsync())
            {
                var roles = new List<Rol>
                {
                    new Rol { nombreRol = "Administrador", descripcion = "Acceso Total" },
                    new Rol { nombreRol = "Usuario", descripcion = "Acceso Limitado" },
                    new Rol {nombreRol= "Cliente", descripcion="Acceso Limitado"}
                };

                await _dbContext.Rol.AddRangeAsync(roles);
                await _dbContext.SaveChangesAsync();
            }
        }
        private async Task InitialzeUsuario()
        {
            if(!await _dbContext.Usuarios.AnyAsync())
            {
                var usuario = new List<Usuario>
                {
                    new Usuario { nombreUsuario= "Abed", email="master@gmail.com", password="$2a$11$ftbwkevIm.kVrZCJtWwVFOnz8EICF6isvJeKp0ZIC.dCNM0t6ZHti", fecha_registro= DateTime.Now, idRol=1}
                };
                await _dbContext.Usuarios.AddRangeAsync(usuario);
                await _dbContext.SaveChangesAsync();
            }
        }

        [HttpGet]
        public IActionResult Registrarse()
        {
            // Mostrar opciones de rol (solo si es necesario)
            ViewBag.Roles = _dbContext.Rol.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrarse(RegistroUsuarioVM usuario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _dbContext.Rol.ToList();
                return View(usuario);
            }

            if (usuario.password != usuario.confirmarpassword)
            {
                ViewBag.Roles = _dbContext.Rol.ToList();
                ViewData["Mensaje"] = "Las contraseñas no coinciden!";
                return View(usuario);
            }

            // Verificar si el email ya existe
            if (await _dbContext.Usuarios.AnyAsync(u => u.email == usuario.email))
            {
                ViewBag.Roles = _dbContext.Rol.ToList();
                ViewData["Mensaje"] = "Este correo electrónico ya está registrado.";
                return View(usuario);
            }

            // Asignar rol por defecto (Usuario) si no se especifica
            var rolUsuario = await _dbContext.Rol.FirstOrDefaultAsync(r =>
                usuario.idRol !=0 ? r.idRol == usuario.idRol : r.nombreRol == "Usuario");

            if (rolUsuario == null)
            {
                ViewBag.Roles = _dbContext.Rol.ToList();
                ViewData["Mensaje"] = "Error al asignar rol de usuario.";
                return View(usuario);
            }

            // Crear hash de la contraseña (IMPORTANTE para seguridad)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuario.password);

            Usuario nuevoUsuario = new Usuario()
            {
                nombreUsuario = usuario.nombreUsuario,
                email = usuario.email,
                password = hashedPassword, // Guardar el hash, no la contraseña en texto plano
                fecha_registro = DateTime.Now,
                idRol = rolUsuario.idRol
            };

            await _dbContext.Usuarios.AddAsync(nuevoUsuario);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registro exitoso. Por favor inicie sesión.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Cerrar sesión previa si existe
            if (User.Identity.IsAuthenticated)
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUsuarioVM usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            var usuarioEncontrado = await _dbContext.Usuarios
                .Include(u => u.rol) // Incluir información del rol
                .FirstOrDefaultAsync(u => u.email == usuario.email);

            // Verificar usuario y contraseña con BCrypt
            if (usuarioEncontrado == null ||
                !BCrypt.Net.BCrypt.Verify(usuario.password, usuarioEncontrado.password))
            {
                ViewData["Mensaje"] = "Credenciales incorrectas";
                return View(usuario);
            }

            // Configurar claims con información del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioEncontrado.idUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuarioEncontrado.email),
                new Claim(ClaimTypes.Name, usuarioEncontrado.nombreUsuario),
                new Claim(ClaimTypes.Role, usuarioEncontrado.rol.nombreRol) // Usar el nombre del rol
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = usuario.recordar, // Opción "recordar sesión"
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Tiempo de sesión
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirigir según rol
            if (usuarioEncontrado.rol.nombreRol == "Administrador")
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize] // Solo usuarios autenticados pueden cerrar sesión
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [Route("Login/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}