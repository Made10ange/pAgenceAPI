using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Controllers;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColisController : AgenceControllerBase
    {
        private readonly IColisRepository _repository;
        private readonly IEcritureRepository _ecriture;
        private readonly ILogger<ColisController> _logger;

        public ColisController(IColisRepository repository, IEcritureRepository ecriture,
                               ILogger<ColisController> logger)
        {
            _repository = repository;
            _ecriture   = ecriture;
            _logger     = logger;
        }

        [HttpGet]
        [HttpGet("liste")]
        public async Task<ActionResult<List<ColisModel>>> GetAll()
        {
            try { return Ok(await _repository.GetAllAsync(AgenceId)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAll colis");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur chargement colis");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ColisModel>> GetById(int id)
        {
            try
            {
                var c = await _repository.GetByIdAsync(id);
                if (c is null) return NotFound();
                return Ok(c);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetById colis id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpGet("par-reference/{reference}")]
        public async Task<ActionResult<ColisModel>> GetByReference(string reference)
        {
            try
            {
                var c = await _repository.GetByReferenceAsync(reference);
                if (c is null) return NotFound();
                return Ok(c);
            }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpGet("par-voyage/{idVoyage}")]
        public async Task<ActionResult<List<ColisModel>>> GetByVoyage(int idVoyage)
        {
            try { return Ok(await _repository.GetByVoyageAsync(idVoyage)); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpGet("par-trajet-voyage/{idVoyage}")]
        public async Task<ActionResult<List<ColisModel>>> GetByTrajetVoyage(int idVoyage)
        {
            try { return Ok(await _repository.GetByTrajetVoyageAsync(idVoyage)); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpGet("par-statut")]
        public async Task<ActionResult<List<ColisModel>>> GetByStatut([FromQuery] string statut)
        {
            try { return Ok(await _repository.GetByStatutAsync(statut)); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpGet("rechercher")]
        public async Task<ActionResult<List<ColisModel>>> Search([FromQuery] string motCle)
        {
            try
            {
                var result = string.IsNullOrWhiteSpace(motCle)
                    ? await _repository.GetAllAsync()
                    : await _repository.SearchAsync(motCle);
                return Ok(result);
            }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpGet("generer-reference")]
        public async Task<ActionResult<string>> GenererReference()
        {
            try { return Ok(await _repository.GenererReferenceAsync()); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpPost("ajouter")]
        public async Task<ActionResult<int>> Add([FromBody] ColisModel colis)
        {
            try
            {
                var idAgenceColis = colis.Id_Agence ?? AgenceId;
                if (colis.Prix_Transport > 0 && !await _ecriture.JourneeOuverteAsync(DateTime.Today, idAgenceColis))
                    return BadRequest(new { message = "Aucune journée comptable ouverte pour votre agence. Impossible d'enregistrer ce colis." });

                var id = await _repository.AddAsync(colis);

                if (colis.Prix_Transport > 0)
                {
                    var numTx = $"COLI-{(string.IsNullOrEmpty(colis.Reference_Colis) ? id.ToString() : colis.Reference_Colis)}";
                    await _ecriture.EcritureColisAsync(
                        numTx, colis.Reference_Colis, colis.Prix_Transport,
                        colis.Nom_Expediteur, colis.Nom_Destinataire,
                        idAgenceColis, UserId);
                }

                return Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Add colis");
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPut("modifier")]
        public async Task<ActionResult<string>> Update([FromBody] ColisModel colis)
        {
            try { return Ok(await _repository.UpdateAsync(colis)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Update colis id={Id}", colis.Id_Colis);
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPatch("statut/{id}")]
        public async Task<ActionResult<string>> UpdateStatut(int id, [FromQuery] string statut)
        {
            try { return Ok(await _repository.UpdateStatutAsync(id, statut)); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            try { return Ok(await _repository.DeleteAsync(id)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Delete colis id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }
    }
}
