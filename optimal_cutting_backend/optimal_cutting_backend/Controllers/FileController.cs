using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
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
        [Route("/import/1d")]
        public async Task<ActionResult> ImportCsv(IFormFile file)
        {
            if (file == null) return BadRequest("file is null");
            if (!IsFileExtensionAllowed(file, new string[] { ".csv" })) return BadRequest("Invalid file type. Please upload a CSV file.");
            var details = _csvService.ReadCSV<DetailOneDivisionDTO>(file.OpenReadStream());
            return Ok(details);
        }

        //check file type
        public static bool IsFileExtensionAllowed(IFormFile file, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName);
            return allowedExtensions.Contains(extension);
        }
    }
}
