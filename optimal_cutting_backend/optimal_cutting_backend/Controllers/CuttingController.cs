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
        /// <summary>
        /// Method for calculating optimal 1d cutting.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Returns the calculated model</returns>
        /// <response code="200">Calculeted is ok</response>
        /// <response code="500">Detail length > workpiece length</response>
        [HttpPost]
        [Route("1d/calculate")]
        public async Task<ActionResult> Calculate1DCutting([FromBody] Calculate1DDTO dto)
        {
            var res = await _cutting1DService.CalculateCuttingAsync(dto.Details, dto.WorkpieceLength);
            return Ok(res);
        }
    }
}
