using Dapper;
using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Controllers;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaiementsController : AgenceControllerBase
    {
        private readonly IPaiementRepository _repo;
        private readonly IEcritureRepository _ecriture;
        public PaiementsController(IPaiementRepository repo, IEcritureRepository ecriture)
        {
            _repo     = repo;
            _ecriture = ecriture;
        }

        [HttpGet("liste")]
        public async Task<IActionResult> Liste() => Ok(await _repo.GetAllAsync(AgenceId));

        // GET api/Paiements/chiffre-affaire-mensuel?annee=2026
        [HttpGet("chiffre-affaire-mensuel")]
        public async Task<IActionResult> ChiffreAffaireMensuel([FromQuery] int? annee = null)
        {
            var a = annee ?? DateTime.Today.Year;
            var data = await _repo.GetChiffreAffaireMensuelAsync(a, AgenceId);
            return Ok(data.Select(d => new { mois = d.Mois, total = d.Total }));
        }

        // GET api/Paiements/recettes  — recettes du jour et du mois depuis la table billet (fiable)
        [HttpGet("recettes")]
        public async Task<IActionResult> Recettes()
        {
            try
            {
                var cs = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString("DefaultConnection")!;
                using var conn = new MySqlConnector.MySqlConnection(cs);

                // Recettes par billet (agence vendeuse ou agence du voyage)
                var idAgence = AgenceId;
                var sql = @"
                    SELECT
                        IFNULL(SUM(CASE WHEN DATE(b.Date_Achat) = CURDATE()                        THEN b.Montant ELSE 0 END), 0) AS RecettesJour,
                        IFNULL(SUM(CASE WHEN MONTH(b.Date_Achat) = MONTH(CURDATE())
                                         AND YEAR(b.Date_Achat)  = YEAR(CURDATE())                 THEN b.Montant ELSE 0 END), 0) AS RecettesMois,
                        SUM(CASE WHEN DATE(b.Date_Achat) = CURDATE()                               THEN 1 ELSE 0 END)             AS NbJour,
                        SUM(CASE WHEN MONTH(b.Date_Achat) = MONTH(CURDATE())
                                  AND YEAR(b.Date_Achat)  = YEAR(CURDATE())                        THEN 1 ELSE 0 END)             AS NbMois
                    FROM billet b
                    LEFT JOIN voyage v ON v.Id_Voyage = b.Id_Voyage_Prevu
                    WHERE (@idAgence IS NULL OR b.Id_Agence = @idAgence OR v.Id_Agence = @idAgence)";

                var r = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { idAgence });
                return Ok(new
                {
                    RecettesJour  = (decimal)(r?.RecettesJour  ?? 0),
                    RecettesMois  = (decimal)(r?.RecettesMois  ?? 0),
                    NombreJour    = (int)(r?.NbJour ?? 0),
                    NombreMois    = (int)(r?.NbMois ?? 0)
                });
            }
            catch (Exception ex)
            {
                return Ok(new { RecettesJour = 0m, RecettesMois = 0m, NombreJour = 0, NombreMois = 0 });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            return p == null ? NotFound() : Ok(p);
        }

        [HttpGet("par-passager/{idPassager}")]
        public async Task<IActionResult> ParPassager(int idPassager)
            => Ok(await _repo.GetByPassagerAsync(idPassager));

        [HttpGet("par-colis/{idColis}")]
        public async Task<IActionResult> ParColis(int idColis)
            => Ok(await _repo.GetByColisAsync(idColis));

        [HttpGet("par-periode")]
        public async Task<IActionResult> ParPeriode([FromQuery] DateTime dateDebut, [FromQuery] DateTime dateFin)
            => Ok(await _repo.GetByPeriodeAsync(dateDebut, dateFin));

        [HttpGet("total-periode")]
        public async Task<IActionResult> TotalPeriode([FromQuery] DateTime dateDebut, [FromQuery] DateTime dateFin)
            => Ok(await _repo.GetTotalByPeriodeAsync(dateDebut, dateFin));

        [HttpGet("rechercher")]
        public async Task<IActionResult> Rechercher([FromQuery] string motCle)
            => Ok(await _repo.SearchAsync(motCle ?? ""));

        [HttpPost("ajouter")]
        public async Task<IActionResult> Ajouter([FromBody] PaiementModel paiement)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _ecriture.JourneeOuverteAsync(DateTime.Today, AgenceId))
                return BadRequest(new { message = "Aucune journée comptable ouverte pour votre agence. Impossible d'enregistrer un paiement." });

            var result = await _repo.AddAsync(paiement);
            return Ok(result);
        }

        [HttpPut("modifier")]
        public async Task<IActionResult> Modifier([FromBody] PaiementModel paiement)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _repo.UpdateAsync(paiement);
            return result.Contains("introuvable") ? NotFound(result) : Ok(result);
        }

        [HttpDelete("supprimer/{id}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var result = await _repo.DeleteAsync(id);
            return result.Contains("introuvable") ? NotFound(result) : Ok(result);
        }
    }
}
