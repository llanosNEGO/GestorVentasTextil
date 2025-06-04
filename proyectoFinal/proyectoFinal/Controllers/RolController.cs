using proyectoFinal.Data;
using proyectoFinal.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;

namespace proyectoFinal.Controllers
{
    public class RolController : Controller
    {
        public readonly AppDBContext _dbContext;
        public RolController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Lista()
        {
            List<Rol> Lista = await _dbContext.Rol.ToListAsync();
            return View(Lista);
        }

    }
}
