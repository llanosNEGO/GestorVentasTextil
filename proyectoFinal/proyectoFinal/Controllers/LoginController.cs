using proyectoFinal.Data;
using proyectoFinal.Models;
using proyectoFinal.ViewM;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;
using Azure.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace proyectoFinal.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDBContext _dbContext;
        public LoginController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registrarse(RegistroUsuarioVM usuario)
        {
            if (usuario.password != usuario.confirmarpassword)
            {
                ViewData["Mensaje"] = "Contraseñas no coinciden!";
                return View(usuario);
            }
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            Usuario usuario1 = new Usuario()
            {
                nombreUsuario = usuario.nombreUsuario,
                email = usuario.email,
                password = usuario.password,
                fecha_registro = usuario.fecha_registro,
                idRol=2
            };

            //Guarda Usuario en DB
            await _dbContext.Usuarios.AddAsync(usuario1);
            await _dbContext.SaveChangesAsync();

            // Verificacion de registro de usuario
            if(usuario1.idUsuario != null)
            {
                TempData["SuccessMessage"] = "Registro exitoso";
                return RedirectToAction("Login", "Login");
            }
            ViewData["Mensaje"] = "Hubo un error en el Registro";
            return View(usuario1);
        }

        [HttpGet]

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Login (LoginUsuarioVM usuario)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {

                    Console.WriteLine($"Error producido {error.ErrorMessage}");
                }
                Console.WriteLine("Error de Formulario");
                ViewData["Mensaje"] = "Por favor complete todos los campos requeridos correctamente";
                return View(usuario);
            }
            var usuarioEncontrado = await _dbContext.Usuarios.FirstOrDefaultAsync(a => a.nombreUsuario == usuario.nombreUsuario &&
                a.password == usuario.password);

            if(usuarioEncontrado == null)
            {
                Console.Write("Datos Ingresados son Incorrectos");
                ViewData["Mensaje"] = "Nombre o Contraseña son incorrectos";
                return View(usuario); 
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuarioEncontrado.nombreUsuario),
                new Claim(ClaimTypes.NameIdentifier, usuarioEncontrado.idUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuarioEncontrado.idRol.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false, // recordar
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            return RedirectToAction("Index", "Home");
        }


    }
}
