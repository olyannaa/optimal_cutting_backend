using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Migrations.EF;
using vega.Services.Interfaces;

namespace vega.Controllers
{
    public class DetailController : Controller
    {
        private readonly VegaContext _db;
        private readonly IDXFService _dxfService;
        private readonly IDrawService _drawService;

        public DetailController(VegaContext db, IDXFService dXFService, IDrawService drawService)
        {
            _db = db;
            _dxfService = dXFService;
            _drawService = drawService;
        }


        /// <summary>
        /// Get all details
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/detail")]
        public async Task<IActionResult> GetDetails()
        {
            return Ok(await _db.Filenames.ToListAsync());
        }

        /// <summary>
        /// Create new detail and generate png file
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/detail")]
        public async Task<IActionResult> CreateDetail(DetailDTO dto, IFormFile file)
        {
            var detail = new Filename
            {
                Name = dto.Name,
                Designation = dto.Designation,
                Thickness = dto.Thickness,
                FileName = dto.Filename,
                MaterialId = dto.MaterialId,
                UserId = dto.UserId,
            };
            if (file.Length == 0) return BadRequest("file is null");
            using var fileStream = file.OpenReadStream();
            byte[] bytes = new byte[file.Length];
            fileStream.Read(bytes, 0, (int)file.Length);
            await _db.Filenames.AddAsync(detail);
            await _db.SaveChangesAsync();

            var details = await _dxfService.GetDXFAsync(bytes);
            await _db.Figures.AddRangeAsync(details.Select(d => new Figure()
            {
                Coordinates = d.Coorditanes,
                TypeId = d.TypeId,
                FilenameId = detail.Id,
            }));
            await _db.SaveChangesAsync();

            var imageBytes = await _drawService.DrawDXFAsync(details);
            return File(imageBytes, "image/png");
        }

        /// <summary>
        /// Get all materials
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/detail/material")]
        public async Task<IActionResult> GetMaterials()
        {
            return Ok(await _db.Materials.ToListAsync());
        }
    }
}
