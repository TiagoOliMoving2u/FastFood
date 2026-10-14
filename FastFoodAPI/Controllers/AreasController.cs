using Microsoft.AspNetCore.Mvc;
using FastFoodAPI.Repositories;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/areas")]
    public class AreasController : ControllerBase
    {
        private readonly AreaRepository _areaRepository;

        public AreasController(AreaRepository areaRepository)
        {
            _areaRepository = areaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas(CancellationToken cancellationToken) 
        {
            var areas = await _areaRepository.GetAllAreas(cancellationToken);

            return Ok(areas);
        }
    }
}
