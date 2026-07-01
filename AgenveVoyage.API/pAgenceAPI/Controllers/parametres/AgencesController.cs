using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencesController : ControllerBase
    {
        private readonly IAgenceRepository _repository;
        private readonly ILogger<AgencesController> _logger;

        public AgencesController(IAgenceRepository repository, ILogger<AgencesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("liste")]
        public async Task<ActionResult<List<AgenceModel>>> GetAll()
        {
            try
            {
                return Ok(await _repository.GetAllAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAll agences");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du chargement des agences");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AgenceModel>> GetById(int id)
        {
            try
            {
                var agence = await _repository.GetByIdAsync(id);
                if (agence is null) return NotFound();
                return Ok(agence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetById agence id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la récupération");
            }
        }

        [HttpGet("rechercher")]
        public async Task<ActionResult<List<AgenceModel>>> Search([FromQuery] string motCle)
        {
            try
            {
                var agences = string.IsNullOrWhiteSpace(motCle)
                    ? await _repository.GetAllAsync()
                    : await _repository.SearchAsync(motCle);
                return Ok(agences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Search agences");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la recherche");
            }
        }

        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] AgenceModel agence)
        {
            try
            {
                var message = await _repository.AddAsync(agence);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Create agence");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de l'ajout");
            }
        }

        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] AgenceModel agence)
        {
            try
            {
                if (id != agence.Id_Agence) return BadRequest("ID incohérent");
                var existing = await _repository.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = $"Agence ID {id} non trouvée" });
                var message = await _repository.UpdateAsync(agence);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Update agence id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la modification");
            }
        }

        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = $"Agence ID {id} non trouvée" });
                var message = await _repository.DeleteAsync(id);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Delete agence id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la suppression");
            }
        }
    }
}
