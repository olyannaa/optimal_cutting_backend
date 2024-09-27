using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Xml.Linq;
using vega.Controllers.DTO;
using vega.Services;

namespace vega.Controllers
{
    [Route("/[controller]")]
    public class FileController : Controller
    {

        private readonly ICSVService _csvService;
        public FileController(ICSVService csvService)
        {
            _csvService = csvService;
        }

        [HttpPost]
        [Route("/1d/import")]
        public async Task<ActionResult> ImportCsv(IFormFile file)
        {
            if (file == null) return BadRequest("file is null");
            if (!IsFileExtensionAllowed(file, new string[] { ".csv" })) return BadRequest("Invalid file type. Please upload a CSV file.");
            var details = _csvService.ReadCSV<DetailOneDivisionDTO>(file.OpenReadStream());
            return Ok(details);
        }

        [HttpPost]
        [Route("/1d/export")]
        public async Task<ActionResult> ExportCsv([FromBody] List<DetailOneDivisionDTO> dto)
        {
            if (dto.Count == 0) return BadRequest("details is null");
            var file = _csvService.WriteCSV(dto);
            return File(file, "application/octet-stream", "export.csv");
        }

        //check file type
        public static bool IsFileExtensionAllowed(IFormFile file, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName);
            return allowedExtensions.Contains(extension);
        }
    }
}
