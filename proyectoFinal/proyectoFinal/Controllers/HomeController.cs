using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proyectoFinal.Models;
using proyectoFinal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace proyectoFinal.Controllers
{
    [Authorize(Roles = "Administrador, Usuario")]
    public class HomeController : Controller
    {
        private readonly AppDBContext _dbContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, AppDBContext appDBContext)
        {
            _logger = logger;
            _dbContext = appDBContext;
        }

        public IActionResult Index()
        {
            var productosDestacados = _dbContext.Productos
                .Include(p => p.categoria)
                .Where(p => p.stock > 0)
                .OrderByDescending(p => p.precioProducto)
                .Take(8)
                .ToList();
            return View(productosDestacados);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}