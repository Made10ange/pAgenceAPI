using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmbarquementsController : ControllerBase
    {
        private readonly IEmbarquementRepository _repository;
        private readonly IVoyageRepository _voyageRepository;
        private readonly IPassagerRepository _passagerRepository;
        private readonly IBilletRepository _billetRepository;
        private readonly ILogger<EmbarquementsController> _logger;

        public EmbarquementsController(
            IEmbarquementRepository repository,
            IVoyageRepository voyageRepository,
            IPassagerRepository passagerRepository,
            IBilletRepository billetRepository,
            ILogger<EmbarquementsController> logger)
        {
            _repository = repository;
            _voyageRepository = voyageRepository;
            _passagerRepository = passagerRepository;
            _billetRepository = billetRepository;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("liste")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetAll()
        {
            try
            {
                return Ok(await _repository.GetAllAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAll embarquements");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du chargement");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmbarquementVoyagePassagerModel>> GetById(int id)
        {
            try
            {
                var embarquement = await _repository.GetByIdAsync(id);
                if (embarquement == null)
                    return NotFound(new { message = $"Embarquement ID {id} non trouvé" });
                return Ok(embarquement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetById embarquement id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la récupération");
            }
        }

        [HttpGet("rechercher")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> Search([FromQuery] string motCle)
        {
            try
            {
                var embarquements = string.IsNullOrWhiteSpace(motCle)
                    ? await _repository.GetAllAsync()
                    : await _repository.SearchAsync(motCle);
                return Ok(embarquements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Search embarquements");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la recherche");
            }
        }

        [HttpGet("par-voyage/{idVoyage}")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetByVoyage(int idVoyage)
        {
            try
            {
                return Ok(await _repository.GetByVoyageAsync(idVoyage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByVoyage idVoyage={IdVoyage}", idVoyage);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du filtre par voyage");
            }
        }

        [HttpGet("par-passager/{idPassager}")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetByPassager(int idPassager)
        {
            try
            {
                return Ok(await _repository.GetByPassagerAsync(idPassager));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByPassager idPassager={IdPassager}", idPassager);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du filtre par passager");
            }
        }

        [HttpGet("par-statut/{statut}")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetByStatut(string statut)
        {
            try
            {
                return Ok(await _repository.GetByStatutAsync(statut));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByStatut statut={Statut}", statut);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du filtre par statut");
            }
        }

        [HttpGet("confirme")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetConfirmes()
        {
            try
            {
                var confirmes = await _repository.GetByStatutAsync("Confirmé");
                var presents = await _repository.GetByStatutAsync("Présent");
                return Ok(confirmes.Concat(presents).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetConfirmes embarquements");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du chargement");
            }
        }

        [HttpGet("verification-siege/{idVoyage}/{numeroSiege}")]
        public async Task<ActionResult<bool>> VerifierDisponibiliteSiege(int idVoyage, int numeroSiege)
        {
            try
            {
                var embarquements = await _repository.GetByVoyageAsync(idVoyage);
                var siegeOccupe = embarquements.Any(e =>
                    e.Numero_Siege == numeroSiege &&
                    (e.Statut_Embarquement == "Confirmé" || e.Statut_Embarquement == "Présent")
                );
                return Ok(!siegeOccupe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur VerifierDisponibiliteSiege voyage={IdVoyage} siege={NumeroSiege}", idVoyage, numeroSiege);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la vérification");
            }
        }

        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] EmbarquementVoyagePassagerModel embarquement)
        {
            try
            {
                var voyage = await _voyageRepository.GetByIdAsync(embarquement.Id_Voyage);
                if (voyage == null)
                    return BadRequest(new { message = $"Voyage ID {embarquement.Id_Voyage} non trouvé" });

                var passager = await _passagerRepository.GetByIdAsync(embarquement.Id_Passager);
                if (passager == null)
                    return BadRequest(new { message = $"Passager ID {embarquement.Id_Passager} non trouvé" });

                embarquement.Date_Enregistrement ??= DateTime.Now;

                var message = await _repository.AddAsync(embarquement);
                // Marquer automatiquement le billet du passager comme "Utilisé"
                await _billetRepository.UtiliserParPassagerAsync(embarquement.Id_Passager, embarquement.Id_Voyage);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Create embarquement");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de l'ajout");
            }
        }

        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] EmbarquementVoyagePassagerModel embarquement)
        {
            try
            {
                if (id != embarquement.Id_Embarquement)
                    return BadRequest(new { message = "ID incohérent" });

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = $"Embarquement ID {id} non trouvé" });

                var message = await _repository.UpdateAsync(embarquement);
                if (embarquement.Statut_Embarquement == "Confirmé")
                    await _billetRepository.UtiliserParPassagerAsync(embarquement.Id_Passager, embarquement.Id_Voyage);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Update embarquement id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la modification");
            }
        }

        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = $"Embarquement ID {id} non trouvé" });

                var message = await _repository.DeleteAsync(id);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Delete embarquement id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la suppression");
            }
        }

        // POST api/Embarquements/valider-groupe
        [HttpPost("valider-groupe")]
        public async Task<ActionResult> ValiderGroupe([FromBody] List<EmbarquementVoyagePassagerModel> embarquements)
        {
            if (embarquements == null || embarquements.Count == 0)
                return BadRequest(new { message = "Aucun embarquement fourni." });

            int inseres = 0;
            int ignores = 0;
            var now = DateTime.Now;

            foreach (var emb in embarquements)
            {
                try
                {
                    // Ignorer si déjà embarqué sur ce voyage
                    var existants = await _repository.GetByVoyageAsync(emb.Id_Voyage);
                    if (existants.Any(e => e.Id_Passager == emb.Id_Passager))
                    {
                        ignores++;
                        continue;
                    }
                    emb.Date_Enregistrement = now;
                    emb.Statut_Embarquement ??= "Confirmé";
                    await _repository.AddAsync(emb);
                    await _billetRepository.UtiliserParPassagerAsync(emb.Id_Passager, emb.Id_Voyage);
                    inseres++;
                }
                catch
                {
                    ignores++;
                }
            }

            return Ok(new { inseres, ignores, message = $"{inseres} passager(s) embarqué(s), {ignores} ignoré(s)." });
        }
    }
}
