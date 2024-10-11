using Microsoft.AspNetCore.Mvc;
using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Migrations.EF;
using vega.Services;
using vega.Services.Interfaces;
using static iTextSharp.text.pdf.AcroFields;

namespace vega.Controllers
{
    public class DetailController : Controller
    {
        private readonly VegaContext _db;
        private readonly IDXFService _dxfService;

        public DetailController(VegaContext db, IDXFService dXFService)
        {
            _db = db;
            _dxfService = dXFService;
        }


        /// <summary>
        /// Get all details
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/detail")]
        public async Task<IActionResult> GetDetails()
        {
            return Ok(_db.Filenames.ToArray());
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
            _db.Filenames.Add(detail);
            _db.SaveChanges();

            var details = _dxfService.GetDXF(bytes);
            _db.Figures.AddRange(details.Select(d => new Figure()
            {
                Coordinates = d.Coorditanes,
                TypeId = d.TypeId,
                FilenameId = detail.Id,
            }));
            _db.SaveChanges();

            var imageBytes = _dxfService.DrawPng(details);
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
            return Ok(_db.Materials.ToArray());
        }
    }
}
