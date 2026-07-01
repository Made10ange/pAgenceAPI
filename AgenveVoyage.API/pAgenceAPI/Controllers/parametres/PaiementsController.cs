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
