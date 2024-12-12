using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Prng;
using vega.Controllers.DTO;
using vega.Migrations.EF;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Controllers
{
    [Route("/api")]
    public class CuttingController : Controller
    {
        private readonly ICutting1DService _cutting1DService;
        private readonly ICutting2DService _cutting2DService;
        private readonly VegaContext _db;
        public CuttingController(ICutting1DService cutting1DService, ICutting2DService cutting2DService, VegaContext db)
        {
            _db = db;
            _cutting1DService = cutting1DService;
            _cutting2DService = cutting2DService;
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
            var res = await _cutting1DService.CalculateCuttingAsync(dto.Details, dto.WorkpiecesLength);
            return Ok(res);
        }

        /// <summary>
        /// Method for calculating optimal 2d cutting.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Returns the calculated model</returns>
        /// <response code="200">Calculeted is ok</response>
        /// <response code="500">Detail length > workpiece length</response>
        [HttpPost]
        [Route("2d/calculate")]
        public async Task<ActionResult> Calculate2DCutting([FromBody] Calculate2DDTO dto)
        {
            var details = new List<Detail2D>();
            foreach (var detail in dto.Details)
                for (var i = 0; i < detail.Count; i++)
                    details.Add(new Detail2D()
                    {
                        Width = detail.Width,
                        Height = detail.Height,
                        X = 0,
                        Y = 0,
                    });
            if (dto.WorkpieceId == null) return BadRequest("workpieceId is null");
            var workpiece = _db.Workpieces.Find(dto.WorkpieceId);
            if (workpiece == null) return BadRequest("workpiece is not found");
            var res = await _cutting2DService.CalculateCuttingAsync(details, workpiece, dto.CuttingThickness);
            return Ok(res);
        }
    }
}
