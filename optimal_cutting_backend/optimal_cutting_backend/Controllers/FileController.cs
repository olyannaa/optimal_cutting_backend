using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Migrations.EF;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Controllers
{
    [Route("/api")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FileController : Controller
    {

        private readonly ICSVService _csvService;
        private readonly IDrawService _drawService;
        private readonly IDXFService _dxfService;
        private readonly VegaContext _db;
        public FileController(ICSVService csvService, IDrawService drawService, IDXFService dxfService, VegaContext db)
        {
            _csvService = csvService;
            _drawService = drawService;
            _dxfService = dxfService;
            _db = db;
        }
        /// <summary>
        /// 1D import csv file and formating him in json
        /// </summary>
        /// <param name="file"></param>
        /// <returns>JSON file with details length and count</returns>
        /// <response code="200">Formatting is ok</response>
        /// <response code="400">Input file is null or Invalid file type. Please upload a CSV file</response>
        [HttpPost]
        [Route("1d/import/csv")]
        public async Task<ActionResult> ImportCsv1D(IFormFile file)
        {
            if (file == null) return StatusCode(400);
            if (!IsFileExtensionAllowed(file, new string[] { ".csv" })) return StatusCode(400);
            var details = _csvService.ReadCSV<Detail1DDTO>(file.OpenReadStream());
            return Ok(details);
        }
        /// <summary>
        /// 1D formating json to csv and export csv file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>JSON file with details length and count</returns>
        /// <response code="200">Export is ok</response>
        /// <response code="400">Details count = 0</response>
        [HttpPost]
        [Route("1d/export/csv")]
        public async Task<ActionResult> ExportCsv1D([FromBody] List<Detail1DDTO> dto)
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
        [Route("1d/export/result/png")]
        public async Task<IActionResult> ExportPng([FromBody] Cutting1DResult dto)
        {
            var imageBytes = await _drawService.Draw1DCuttingAsync(dto);
            return File(imageBytes, "image/png");
        }

        /// <summary>
        /// download pdf scheme 1d cutting calculating
        /// </summary>
        /// <returns>pdf file</returns>
        [HttpPost]
        [Route("1d/export/result/pdf")]
        public async Task<IActionResult> ExportPdf([FromBody] Cutting1DResult dto)
        {
            var imageBytes = await _drawService.Draw1DCuttingAsync(dto);
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

                var pdfData = ms.ToArray();
                return File(pdfData, "application/octet-stream", "export.pdf");
            }
        }

        /// <summary>
        /// download csv file result 1d cutting calculating
        /// </summary>
        /// <returns>pdf file</returns>
        [HttpPost]
        [Route("1d/export/result/csv")]
        public async Task<IActionResult> ExportResultCSV([FromBody] Cutting1DResult dto)
        {
            var file = _csvService.WriteCSV(dto.Workpieces);
            return File(file, "application/octet-stream", "result.csv");

        }

        /// <summary>
        /// 2D import csv file and formating him in json
        /// </summary>
        /// <param name="file"></param>
        /// <returns>JSON file with details length and count</returns>
        /// <response code="200">Formatting is ok</response>
        /// <response code="400">Input file is null or Invalid file type. Please upload a CSV file</response>
        [HttpPost]
        [Route("2d/import/csv")]
        public async Task<ActionResult> ImportCsv2D(IFormFile file)
        {
            if (file == null) return StatusCode(400);
            if (!IsFileExtensionAllowed(file, new string[] { ".csv" })) return StatusCode(400);
            var details = _csvService.ReadCSV<Detail2DDTO>(file.OpenReadStream());
            return Ok(details);
        }
        /// <summary>
        /// 2D formating json to csv and export csv file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>JSON file with details length and count</returns>
        /// <response code="200">Export is ok</response>
        /// <response code="400">Details count = 0</response>
        [HttpPost]
        [Route("2d/export/csv")]
        public async Task<ActionResult> ExportCsv2D([FromBody] List<Detail2DDTO> dto)
        {
            if (dto.Count == 0) return BadRequest("details is null");
            var file = _csvService.WriteCSV(dto);
            return File(file, "application/octet-stream", "export.csv");
        }

        /// <summary>
        /// draw png scheme 2d cutting caltulating
        /// </summary>
        /// <returns>png scheme cutting</returns>
        [HttpPost]
        [Route("2d/export/result/png")]
        public async Task<IActionResult> ExportPng([FromBody] Cutting2DResult dto)
        {
            var imageBytes = await _drawService.Draw2DCuttingAsync(dto);

            using (var ms = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var imageNumber = 1;
                    foreach (var image in imageBytes)
                    {
                        var entry = zipArchive.CreateEntry($"Заготовка {imageNumber++}.png");
                        using var stream = entry.Open();
                        await stream.WriteAsync(image, 0, image.Length);
                    }

                }
                ms.Position = 0;
                return File(ms.ToArray(), "application/zip", "Заготовки PNG");
            }
        }

        /// <summary>
        /// draw dxf 2d cutting caltulating
        /// </summary>
        /// <returns>png scheme cutting</returns>
        [HttpPost]
        [Route("2d/export/result/dxf")]
        public async Task<IActionResult> ExportDxf([FromBody] Cutting2DResult dto)
        {
            var dxfBytes = await _dxfService.Create2DDXFAsync(dto);
            using (var ms = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var fileNumber = 1;
                    foreach (var dxf in dxfBytes)
                    {
                        var entry = zipArchive.CreateEntry($"Заготовка {fileNumber++}.dxf");
                        using var stream = entry.Open();
                        await stream.WriteAsync(dxf, 0, dxf.Length);
                    }
                }
                ms.Position = 0;
                return File(ms.ToArray(), "application/zip", "Заготовки DXF");
            }
        }

        /// <summary>
        /// download pdf scheme 2d cutting calculating
        /// </summary>
        /// <returns>pdf file</returns>
        [HttpPost]
        [Route("2d/export/result/pdf")]
        public async Task<IActionResult> Export2DPdf([FromBody] Cutting2DResult dto)
        {
            var imagesBytes = await _drawService.Draw2DCuttingAsync(dto);
            using (var ms = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();
                var table = new PdfPTable(1);
                foreach (var imageBytes in imagesBytes)
                {
                    var image = Image.GetInstance(imageBytes);
                    table.AddCell(image);
                }
                document.Add(table);
                document.Close();

                var pdfData = ms.ToArray();
                return File(pdfData, "application/octet-stream", "export.pdf");
            }
        }

        /// <summary>
        /// draw png scheme dxf cutting caltulating
        /// </summary>
        /// <returns>png scheme cutting</returns>
        [HttpPost]
        [Route("dxf/export/result/png")]
        public async Task<IActionResult> ExportDXFPng([FromBody] Cutting2DResult dto)
        {
            var imageBytes = await _drawService.DrawDXFCuttingAsync(dto);

            using (var ms = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var imageNumber = 1;
                    foreach (var image in imageBytes)
                    {
                        var entry = zipArchive.CreateEntry($"Заготовка {imageNumber++}.png");
                        using var stream = entry.Open();
                        await stream.WriteAsync(image, 0, image.Length);
                    }

                }
                ms.Position = 0;
                return File(ms.ToArray(), "application/zip", "Заготовки PNG");
            }
        }

        /// <summary>
        /// download pdf scheme dxf cutting calculating
        /// </summary>
        /// <returns>pdf file</returns>
        [HttpPost]
        [Route("dxf/export/result/pdf")]
        public async Task<IActionResult> ExportDxfPdf([FromBody] Cutting2DResult dto)
        {
            var imagesBytes = await _drawService.DrawDXFCuttingAsync(dto);
            using (var ms = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();
                var table = new PdfPTable(1);
                foreach (var imageBytes in imagesBytes)
                {
                    var image = Image.GetInstance(imageBytes);
                    table.AddCell(image);
                }
                document.Add(table);
                document.Close();

                var pdfData = ms.ToArray();
                return File(pdfData, "application/octet-stream", "export.pdf");
            }
        }

        /// <summary>
        /// draw dxf file dxf cutting caltulating
        /// </summary>
        /// <returns>png scheme cutting</returns>
        [HttpPost]
        [Route("dxf/export/result/dxf")]
        public async Task<IActionResult> ExportDxfFile([FromBody] Cutting2DResult dto)
        {
            var dxfBytes = await _dxfService.Create2DDXFAsync(dto);
            using (var ms = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var fileNumber = 1;
                    foreach (var dxf in dxfBytes)
                    {
                        var entry = zipArchive.CreateEntry($"Заготовка {fileNumber++}.dxf");
                        using var stream = entry.Open();
                        await stream.WriteAsync(dxf, 0, dxf.Length);
                    }
                }
                ms.Position = 0;
                return File(ms.ToArray(), "application/zip", "Заготовки DXF");
            }
        }

        /// <summary>
        /// DXF import csv file and formating him in json
        /// </summary>
        /// <param name="file"></param>
        /// <returns>JSON file with details ids and count</returns>
        [HttpPost]
        [Route("dxf/import/csv")]
        public async Task<IActionResult> ImportCsvDXF(IFormFile file)
        {
            if (file == null) return StatusCode(400);
            if (!IsFileExtensionAllowed(file, new string[] { ".csv" })) return StatusCode(400);
            var details = _csvService.ReadCSV<DetailDxfDTO>(file.OpenReadStream()).ToList();

            foreach (var detail in details)
                if (!_db.Filenames.Any(f => f.Id == detail.Id)) return StatusCode(400);
            return Ok(details);
        }
        /// <summary>
        /// DXF formating json to csv and export csv file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>JSON file with details id  and count</returns>
        /// <response code="200">Export is ok</response>
        /// <response code="400">Details count = 0</response>
        [HttpPost]
        [Route("dxf/export/csv")]
        public async Task<IActionResult> ExportCsvDXF([FromBody] List<DetailDxfDTO> dto)
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
