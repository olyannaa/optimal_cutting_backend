using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Xml.Linq;
using vega.Controllers.DTO;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Controllers
{
    [Route("/[controller]")]
    public class FileController : Controller
    {

        private readonly ICSVService _csvService;
        private readonly IDrawService _drawService;
        public FileController(ICSVService csvService, IDrawService drawService)
        {
            _csvService = csvService;
            _drawService = drawService;
        }

        [HttpPost]
        [Route("/1d/importCsv")]
        public async Task<ActionResult> ImportCsv(IFormFile file)
        {
            if (file == null) return BadRequest("file is null");
            if (!IsFileExtensionAllowed(file, new string[] { ".csv" })) return BadRequest("Invalid file type. Please upload a CSV file.");
            var details = _csvService.ReadCSV<DetailOneDivisionDTO>(file.OpenReadStream());
            return Ok(details);
        }

        [HttpPost]
        [Route("/1d/exportCsv")]
        public async Task<ActionResult> ExportCsv([FromBody] List<DetailOneDivisionDTO> dto)
        {
            if (dto.Count == 0) return BadRequest("details is null");
            var file = _csvService.WriteCSV(dto);
            return File(file, "application/octet-stream", "export.csv");
        }

        [HttpPost]
        [Route("/1d/exportPng")]
        public async Task<IActionResult> ExportPng([FromBody] Cutting1DResult dto)
        {
            var imageBytes = _drawService.Draw1DCutting(dto);
            return File(imageBytes, "image/png");
        }

        //check file type
        public static bool IsFileExtensionAllowed(IFormFile file, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName);
            return allowedExtensions.Contains(extension);
        }
    }
}
