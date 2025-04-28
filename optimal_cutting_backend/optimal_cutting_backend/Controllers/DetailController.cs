using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Migrations.EF;
using vega.Services.Interfaces;

namespace vega.Controllers
{
    [Route("/api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DetailController : Controller
    {
        private readonly VegaContext _db;
        private readonly IDXFService _dxfService;
        private readonly IDrawService _drawService;
        private readonly IHttpContextAccessor _accessor;

        public DetailController(VegaContext db, IDXFService dXFService, IDrawService drawService, IHttpContextAccessor accessor)
        {
            _db = db;
            _dxfService = dXFService;
            _drawService = drawService;
            _accessor = accessor;
        }


        /// <summary>
        /// Get all details
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDetails()
        {
            return Ok(await _db.Filenames.ToListAsync());
        }

        /// <summary>
        /// Get details' designations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("designations")]
        public async Task<IActionResult> GetDesignations()
        {
            var filenames = await _db.Filenames.ToArrayAsync();

            return Ok(
                 filenames
                 .Select(x => new { x.Id, x.Designation, x.Thickness, x.MaterialId })
                 .OrderBy(x => x.Designation)
                 .GroupBy(x => new string(x.Designation.TakeWhile(x => x != '.').ToArray()))
                 .ToDictionary(x => x.Key, x => x.ToList()));
        }

        /// <summary>
        /// Create new detail and generate png file
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateDetail([FromForm] DetailDTO dto)
        {
            if (await _db.Filenames.AnyAsync(x => x.Designation == dto.Designation))
                return BadRequest("Detail with this designation already exists");

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("File is empty");

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                byte[] fileBytes;
                using (var stream = dto.File.OpenReadStream())
                {
                    fileBytes = new byte[dto.File.Length];
                    await stream.ReadAsync(fileBytes, 0, (int)dto.File.Length);
                }

                if (!IsValidDXF(fileBytes))
                {
                    throw new Exception("Invalid DXF file format");
                }

                
                var detail = new Filename
                {
                    Name = dto.Name,
                    Designation = dto.Designation,
                    Thickness = dto.Thickness,
                    FileName = dto.Filename,
                    MaterialId = dto.MaterialId,
                    UserId = Int32.Parse(_accessor.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Sid).Value)
                };

                await _db.Filenames.AddAsync(detail);
                await _db.SaveChangesAsync();

                
                var figures = await _dxfService.GetDXFAsync(fileBytes);


                var entities = figures.Select(f => new Figure
                {
                    Coordinates = f.Coordinates,
                    TypeId = f.TypeId,
                    FilenameId = detail.Id
                }).ToList();

                await _db.Figures.AddRangeAsync(entities);
                await _db.SaveChangesAsync();

                
                var imageBytes = await _drawService.DrawDXFAsync(entities);

                
                await transaction.CommitAsync();

                return File(imageBytes, "image/png");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error processing DXF file: {ex.Message}");
            }
        }

        private bool IsValidDXF(byte[] fileBytes)
        {
            try
            {
                var text = System.Text.Encoding.ASCII.GetString(fileBytes, 0, Math.Min(100, fileBytes.Length));
                return text.Contains("SECTION") && text.Contains("HEADER");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete Details by id
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> DeleteDetails([FromBody] DeleteDetailDTO dto)
        {
            var details = await _db.Filenames.Where(f => dto.DetailsIds.Contains(f.Id)).Include(f => f.Figures).ToListAsync();
            _db.Filenames.RemoveRange(details);
            await _db.SaveChangesAsync();
            return Ok(new DeleteDetailResponse{ Message = "Детали удалены"});
        }

        /// <summary>
        /// Get all materials
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("material")]
        public async Task<IActionResult> GetMaterials()
        {
            return Ok(await _db.Materials.ToListAsync());
        }

        /// <summary>
        /// Create new workpiece
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("workpiece")]
        public async Task<IActionResult> CreateWorkpiece([FromBody] WorkpieceDTO dto)
        {
            var workpiece = new Migrations.DAL.Workpiece
            {
                Name = dto.Name,
                Width = dto.Width,
                Height = dto.Height,
            };
            await _db.Workpieces.AddAsync(workpiece);
            await _db.SaveChangesAsync();

            return Ok(workpiece);
        }

        /// <summary>
        /// Get all workpieces
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("workpiece")]
        public async Task<IActionResult> GetWorkpieces()
        {
            return Ok(await _db.Workpieces.ToListAsync());
        }
    }
}
