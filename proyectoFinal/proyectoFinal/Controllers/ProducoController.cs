using proyectoFinal.Data;
using proyectoFinal.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;

namespace proyectoFinal.Controllers
{
    public class ProductoController : Controller
    {
        private readonly AppDBContext _dbContext;
        public ProductoController(AppDBContext appDBContext)
        {
            _dbContext = appDBContext;
        }
        [HttpGet]

        public async Task<IActionResult> Lista()
        {
            List<Producto> lista = await _dbContext.Productos.ToListAsync();
            return View(lista);
        }

    }
}
