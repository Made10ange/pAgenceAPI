using Microsoft.AspNetCore.Mvc;
using pAgenceAPI;
using Microsoft.EntityFrameworkCore;

namespace pAgenceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/test/connection
        [HttpGet("connection")]
        public async Task<ActionResult<string>> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    return Ok("✅ Connexion à la base de données réussie !");
                }
                else
                {
                    return BadRequest("❌ Échec de connexion à la base de données");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"❌ Erreur : {ex.Message}");
            }
        }

        // GET: api/test/tables
        [HttpGet("tables")]
        public async Task<ActionResult<List<string>>> GetTables()
        {
            try
            {
                var tables = await _context.Database
                    .SqlQueryRaw<string>("SHOW TABLES")
                    .ToListAsync();

                return Ok(tables);
            }
            catch (Exception ex)
            {
                return BadRequest($"❌ Erreur : {ex.Message}");
            }
        }
    }
}
