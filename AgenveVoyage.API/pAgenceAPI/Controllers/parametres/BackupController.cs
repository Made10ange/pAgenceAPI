using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Services;

namespace pAgenceAPI.Controllers.parametres
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly BackupService _backup;
        private readonly IConfiguration _config;

        public BackupController(BackupService backup, IConfiguration config)
        {
            _backup = backup;
            _config = config;
        }

        [HttpPost("lancer")]
        public async Task<IActionResult> Lancer()
        {
            var (ok, message, fichier) = await _backup.FaireBackupAsync();
            return Ok(new { ok, message, fichier });
        }

        [HttpGet("liste")]
        public IActionResult Liste()
        {
            var dossier = _config["Backup:Dossier"] ?? Path.Combine(AppContext.BaseDirectory, "Backups");
            if (!Directory.Exists(dossier)) return Ok(new List<object>());

            var fichiers = Directory.GetFiles(dossier, "*.sql")
                .Select(f => new
                {
                    Nom     = Path.GetFileName(f),
                    Taille  = new FileInfo(f).Length,
                    Date    = System.IO.File.GetCreationTime(f)
                })
                .OrderByDescending(f => f.Date)
                .ToList();

            return Ok(fichiers);
        }

        [HttpGet("telecharger/{nom}")]
        public IActionResult Telecharger(string nom)
        {
            var dossier = _config["Backup:Dossier"] ?? Path.Combine(AppContext.BaseDirectory, "Backups");
            var chemin  = Path.Combine(dossier, nom);
            if (!System.IO.File.Exists(chemin)) return NotFound();
            return PhysicalFile(chemin, "application/octet-stream", nom);
        }
    }
}
