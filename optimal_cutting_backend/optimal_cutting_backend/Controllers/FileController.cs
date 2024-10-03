using CsvHelper;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
        /// <summary>
        /// import csv file and formating him in json
        /// </summary>
        /// <param name="file"></param>
        /// <returns>JSON file with details length and count</returns>
        /// <response code="200">Formatting is ok</response>
        /// <response code="400">Input file is null or Invalid file type. Please upload a CSV file</response>
        [HttpPost]
        [Route("/1d/import/csv")]
        public async Task<ActionResult> ImportCsv(IFormFile file)
        {
            if (file == null) return StatusCode(400);
            if (!IsFileExtensionAllowed(file, new string[] { ".csv" })) return StatusCode(401);
            var details = _csvService.ReadCSV<DetailOneDivisionDTO>(file.OpenReadStream());
            return Ok(details);
        }
        /// <summary>
        /// formating json to csv and export csv file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>JSON file with details length and count</returns>
        /// <response code="200">Export is ok</response>
        /// <response code="400">Details count = 0</response>
        [HttpPost]
        [Route("/1d/export/csv")]
        public async Task<ActionResult> ExportCsv([FromBody] List<DetailOneDivisionDTO> dto)
        {
            if (dto.Count == 0) return BadRequest("details is null");
            var file = _csvService.WriteCSV(dto);
            return File(file, "application/octet-stream", "export.csv");
        }

        /// <summary>
        /// draw png scheme 1d cutting caltulating
        /// </summary>
        /// <returns>png scheme cutting</returns>
        [HttpPost]
        [Route("/1d/export/png")]
        public async Task<IActionResult> ExportPng([FromBody] Cutting1DResult dto)
        {
            var imageBytes = _drawService.Draw1DCutting(dto);
            return File(imageBytes, "image/png");
        }

        /// <summary>
        /// download pdf scheme 1d cutting calculating
        /// </summary>
        /// <returns>pdf file</returns>
        [HttpPost]
        [Route("/1d/export/pdf")]
        public async Task<IActionResult> ExportPdf([FromBody] Cutting1DResult dto)
        {
            var imageBytes = _drawService.Draw1DCutting(dto);
            using (var ms = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();
                var image = Image.GetInstance(imageBytes);
                var table = new PdfPTable(1);
                table.AddCell(image);
                document.Add(table);
                document.Close();

                byte[] pdfData = ms.ToArray();
                return File(pdfData, "application/octet-stream", "export.pdf");
            }
        }

        //check file type
        public static bool IsFileExtensionAllowed(IFormFile file, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName);
            return allowedExtensions.Contains(extension);
        }
    }
}
