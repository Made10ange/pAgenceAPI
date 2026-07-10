using Dapper;
using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Controllers;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class BagagesController : AgenceControllerBase
    {
        private readonly IBagageRepository _repository;
        private readonly IBilletRepository _billetRepository;
        private readonly IEcritureRepository _ecriture;
        private readonly ILogger<BagagesController> _logger;

        public BagagesController(IBagageRepository repository, IBilletRepository billetRepository,
                                 IEcritureRepository ecriture, ILogger<BagagesController> logger)
        {
            _repository       = repository;
            _billetRepository = billetRepository;
            _ecriture         = ecriture;
            _logger           = logger;
        }

        [HttpGet]
        [HttpGet("liste")]
        public async Task<ActionResult<List<BagageModel>>> GetAll()
        {
            try { return Ok(await _repository.GetAllAsync(AgenceId)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAll bagages");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur chargement bagages");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BagageModel>> GetById(int id)
        {
            try
            {
                var b = await _repository.GetByIdAsync(id);
                if (b is null) return NotFound();
                return Ok(b);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetById bagage id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpGet("par-passager/{idPassager}")]
        public async Task<ActionResult<List<BagageModel>>> GetByPassager(int idPassager)
        {
            try { return Ok(await _repository.GetByPassagerAsync(idPassager)); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpGet("par-voyage/{idVoyage}")]
        public async Task<ActionResult<List<BagageModel>>> GetByVoyage(int idVoyage)
        {
            try { return Ok(await _repository.GetByVoyageAsync(idVoyage)); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpGet("rechercher")]
        public async Task<ActionResult<List<BagageModel>>> Search([FromQuery] string motCle)
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

        [HttpPost("ajouter")]
        public async Task<ActionResult<int>> Add([FromBody] BagageModel bagage)
        {
            try
            {
                if (bagage.Montant_Total.HasValue && bagage.Montant_Total.Value > 0
                    && await _ecriture.GetDateJourneeOuverteAsync(AgenceId) is null)
                    return BadRequest(new { message = "Aucune journée comptable ouverte pour votre agence. Impossible d'enregistrer ce bagage." });

                var id = await _repository.AddAsync(bagage);

                if (bagage.Montant_Total.HasValue && bagage.Montant_Total.Value > 0)
                {
                    var numTx   = $"BAG-{id}-{DateTime.Now:yyyyMMddHHmmss}";
                    var passager = bagage.Nom_Passager;
                    await _ecriture.EcritureBagageAsync(
                        numTx, id.ToString(), bagage.Montant_Total.Value,
                        passager, AgenceId, UserId);
                }

                return Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Add bagage");
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPut("modifier")]
        public async Task<ActionResult<string>> Update([FromBody] BagageModel bagage)
        {
            try { return Ok(await _repository.UpdateAsync(bagage)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Update bagage id={Id}", bagage.Id_Bagage);
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
                _logger.LogError(ex, "Erreur Delete bagage id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // POST /api/Bagages/enregistrer-par-passager
        // Enregistre tous les bagages d'un passager en une transaction atomique
        [HttpPost("enregistrer-par-passager")]
        public async Task<ActionResult> EnregistrerParPassager([FromBody] BagageParPassagerRequest req)
        {
            try
            {
                if (req.Montant_Total > 0 && await _ecriture.GetDateJourneeOuverteAsync(AgenceId) is null)
                    return BadRequest(new { message = "Aucune journée comptable ouverte pour votre agence. Impossible d'enregistrer ces bagages." });

                await _repository.EnregistrerParPassagerAsync(req.Id_Passager, req.Id_Voyage, req.Montant_Total, req.Bagages);

                if (req.Montant_Total > 0)
                {
                    var numTx = $"BAGP-{req.Id_Passager}-{DateTime.Now:yyyyMMddHHmmss}";
                    await _ecriture.EcritureBagageAsync(
                        numTx, $"Passager#{req.Id_Passager}", req.Montant_Total.GetValueOrDefault(),
                        null, AgenceId, null);
                }

                return Ok(new { message = $"{req.Bagages.Count} bagage(s) enregistré(s) avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur EnregistrerParPassager idPassager={Id}", req.Id_Passager);
                return Problem(detail: "Erreur transaction — aucun bagage enregistré.", statusCode: 500);
            }
        }

        // GET /api/Bagages/par-voyage-passagers/{idVoyage}
        [HttpGet("par-voyage-passagers/{idVoyage}")]
        public async Task<ActionResult<List<PassagerAvecBagagesDto>>> GetPassagersAvecBagages(int idVoyage)
        {
            try { return Ok(await _repository.GetPassagersAvecBagagesAsync(idVoyage)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetPassagersAvecBagages idVoyage={Id}", idVoyage);
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET /api/Bagages/par-passager-voyage/{idPassager}/{idVoyage}
        [HttpGet("par-passager-voyage/{idPassager}/{idVoyage}")]
        public async Task<ActionResult<List<BagageModel>>> GetParPassagerVoyage(int idPassager, int idVoyage)
        {
            try
            {
                var tous = await _repository.GetByPassagerAsync(idPassager);
                return Ok(tous.Where(b => b.Id_Voyage_Passager == idVoyage).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetParPassagerVoyage");
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpGet("par-embarquement/{idVoyage}")]
        public async Task<ActionResult<List<BagageModel>>> GetByPassagersEmbarques(int idVoyage)
        {
            try
            {
                var billets = await _billetRepository.GetPourEmbarquementAsync(idVoyage);
                var passagerIds = billets
                    .Where(b => b.Id_Passager > 0)
                    .Select(b => b.Id_Passager)
                    .Distinct()
                    .ToList();

                if (!passagerIds.Any())
                    return Ok(new List<BagageModel>());

                var bagages = await _repository.GetByPassagerIdsAsync(passagerIds);
                return Ok(bagages);
            }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpGet("debug-embarquement/{idVoyage}")]
        public async Task<IActionResult> DebugEmbarquement(int idVoyage)
        {
            try
            {
                var cs = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")!;
                using var db = new MySqlConnector.MySqlConnection(cs);
                await db.OpenAsync();

                var billets = await _billetRepository.GetPourEmbarquementAsync(idVoyage);
                var passagerIds = billets.Where(b => b.Id_Passager > 0).Select(b => b.Id_Passager).Distinct().ToList();

                var bagagesEnAttente = passagerIds.Any()
                    ? (await db.QueryAsync<dynamic>(
                        "SELECT ID_bagage, ID_passager, STATUT, DESCRIPTION FROM bagage WHERE ID_passager IN @ids",
                        new { ids = passagerIds })).ToList()
                    : new List<dynamic>();

                var tousLesBagages = (await db.QueryAsync<dynamic>(
                    "SELECT ID_bagage, ID_passager, STATUT, DESCRIPTION FROM bagage WHERE STATUT='En attente' LIMIT 10")).ToList();

                return Ok(new {
                    idVoyage,
                    nbBillets = billets.Count(),
                    passagerIds,
                    bagagesTrouves = bagagesEnAttente,
                    tousLesBagagesEnAttente = tousLesBagages
                });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("en-attente")]
        public async Task<ActionResult<List<BagageModel>>> GetEnAttente()
        {
            try { return Ok(await _repository.GetEnAttenteAsync()); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpPatch("assigner-voyage/{idBagage}")]
        public async Task<ActionResult<string>> AssignerVoyage(int idBagage, [FromQuery] int idVoyage)
        {
            try { return Ok(await _repository.AssignerVoyageAsync(idBagage, idVoyage)); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }

        [HttpGet("archives")]
        public async Task<ActionResult<List<BagageModel>>> GetArchives()
        {
            try { return Ok(await _repository.GetArchivesAsync(AgenceId)); }
            catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
        }
    }
}
