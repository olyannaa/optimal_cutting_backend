using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Migrations.EF;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Controllers
{
    [Route("/api")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            if (dto.Details.Max() > dto.WorkpiecesLength.Max())
                return BadRequest("detail length > workpiece length");
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
            var workpiece = new Workpiece()
            {
                Width = dto.Workpiece.Width,
                Height = dto.Workpiece.Height,
            };
            if (details.Max(d => d.Width) > workpiece.Width || details.Max(d => d.Height) > Math.Max(workpiece.Height, workpiece.Width)) return BadRequest("detail > workpiece");
            var res = await _cutting2DService.CalculateCuttingAsync(details, workpiece, dto.CuttingThickness);
            return Ok(res);
        }

        /// <summary>
        /// Method for calculating optimal 2d cutting with dxf
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Returns the calculated model</returns>
        /// <response code="200">Calculeted is ok</response>
        /// <response code="500">Detail length > workpiece length</response>
        [HttpPost]
        [Route("dxf/calculate")]
        public async Task<ActionResult> Calculate2DCuttingWithDXF([FromBody] Calculate2DDXFDTO dto)
        {
            //Get details from DB and calculate sizes (width, height)
            var details = dto.DetailsId
                .Select(d => _db.Filenames.Include(f => f.Figures).FirstOrDefault(f => f.Id == d))
                .Select(d => new Detail2D(d.Figures))
                .ToList();
            var workpiece = new Workpiece() { Height = dto.Workpiece.Height, Width = dto.Workpiece.Width };
            if (details.Max(d => d.Width) > workpiece.Width || details.Max(d => d.Height) > Math.Max(workpiece.Height, workpiece.Width)) return BadRequest("detail > workpiece");
            var res = await _cutting2DService.CalculateCuttingAsync(details, workpiece, dto.CuttingThickness);
            return Ok(res);
        }
    }
}
