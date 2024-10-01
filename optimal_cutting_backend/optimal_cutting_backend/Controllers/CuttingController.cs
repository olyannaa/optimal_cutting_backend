using Microsoft.AspNetCore.Mvc;
using vega.Controllers.DTO;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Controllers
{
    public class CuttingController : Controller
    {
        private readonly ICutting1DService _cutting1DService;
        public CuttingController(ICutting1DService cutting1DService)
        {
            _cutting1DService = cutting1DService;
        }

        [HttpPost]
        [Route("1d/calculate")]
        public ActionResult Calculate1DCutting([FromBody] Calculate1DDTO dto)
        {
            var res = _cutting1DService.CalculateCutting(dto.Details, dto.Workpieces);
            return Ok(res);
        }
    }
}
