using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChauffeursController : AgenceControllerBase
    {
        private readonly IChauffeurRepository _repository;
        private readonly ILogger<ChauffeursController> _logger;

        public ChauffeursController(IChauffeurRepository repository, ILogger<ChauffeursController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("liste")]
        public async Task<ActionResult<List<ChauffeurModel>>> GetAll()
        {
            try
            {
                return Ok(await _repository.GetAllAsync(AgenceId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAll chauffeurs");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du chargement des chauffeurs");
            }
        }

        [HttpGet("disponibles")]
        public async Task<ActionResult<List<ChauffeurModel>>> GetDisponibles()
        {
            try
            {
                return Ok(await _repository.GetDisponiblesAsync(AgenceId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetDisponibles chauffeurs");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du chargement des chauffeurs disponibles");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChauffeurModel>> GetById(int id)
        {
            try
            {
                var chauffeur = await _repository.GetByIdAsync(id);
                if (chauffeur == null) return NotFound();
                return Ok(chauffeur);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetById chauffeur id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la récupération");
            }
        }

        [HttpGet("rechercher")]
        public async Task<ActionResult<List<ChauffeurModel>>> Search([FromQuery] string motCle)
        {
            try
            {
                var chauffeurs = string.IsNullOrWhiteSpace(motCle)
                    ? await _repository.GetAllAsync()
                    : await _repository.SearchAsync(motCle);
                return Ok(chauffeurs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Search chauffeurs");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la recherche");
            }
        }

        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] ChauffeurModel chauffeur)
        {
            try
            {
                chauffeur.Id_Agence = AgenceId;
                var message = await _repository.AddAsync(chauffeur);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Create chauffeur");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de l'ajout");
            }
        }

        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] ChauffeurModel chauffeur)
        {
            try
            {
                if (id != chauffeur.Id_Chauffeur) return BadRequest("ID incohérent");
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null) return NotFound(new { message = $"Chauffeur ID {id} non trouvé" });
                var message = await _repository.UpdateAsync(chauffeur);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Update chauffeur id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la modification");
            }
        }

        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null) return NotFound(new { message = $"Chauffeur ID {id} non trouvé" });
                var message = await _repository.DeleteAsync(id);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Delete chauffeur id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la suppression");
            }
        }
    }
}
