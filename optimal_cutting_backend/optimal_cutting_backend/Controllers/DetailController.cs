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
            var filename = _db.Filenames.FirstOrDefault(x => x.Designation == dto.Designation);
            if (filename != null) return BadRequest("detail with this designation is found");

            var userId = Int32.Parse(_accessor.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Sid).Value);

            var detail = new Filename
            {
                Name = dto.Name,
                Designation = dto.Designation,
                Thickness = dto.Thickness,
                FileName = dto.Filename,
                MaterialId = dto.MaterialId,
                UserId = userId,
            };
            if (dto.File.Length == 0) return BadRequest("file is null");
            await _db.Filenames.AddAsync(detail);
            await _db.SaveChangesAsync();

            using var fileStream = dto.File.OpenReadStream();
            byte[] bytes = new byte[dto.File.Length];
            fileStream.Read(bytes, 0, (int)dto.File.Length);
            var details = await _dxfService.GetDXFAsync(bytes);
            await _db.Figures.AddRangeAsync(details.Select(d => new Figure()
            {
                Coordinates = d.Coordinates,
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
