using FastFoodAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/areas")]
    public class AreasController : ControllerBase
    {
        private FastFoodDbContext _context;

        public AreasController(FastFoodDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas() 
        {
            var areas = await _context.Areas
                .AsNoTracking()
                .OrderBy(a => a.Id)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Acronym,
                    a.LastOrderNumber
                })
                .ToListAsync();

            return Ok(areas);
        }
    }
}
