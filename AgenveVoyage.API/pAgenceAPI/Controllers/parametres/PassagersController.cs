using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassagersController : AgenceControllerBase
    {
        private readonly IPassagerRepository _repository;
        private readonly ILogger<PassagersController> _logger;

        public PassagersController(IPassagerRepository repository, ILogger<PassagersController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("liste")]
        public async Task<ActionResult<List<PassagerModel>>> GetAll()
        {
            try
            {
                return Ok(await _repository.GetAllAsync(AgenceId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAll passagers");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du chargement des passagers");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PassagerModel>> GetById(int id)
        {
            try
            {
                var passager = await _repository.GetByIdAsync(id);
                if (passager == null) return NotFound();
                return Ok(passager);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetById passager id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la récupération");
            }
        }

        [HttpGet("rechercher")]
        public async Task<ActionResult<List<PassagerModel>>> Search([FromQuery] string motCle)
        {
            try
            {
                var passagers = string.IsNullOrWhiteSpace(motCle)
                    ? await _repository.GetAllAsync(AgenceId)
                    : await _repository.SearchAsync(motCle, AgenceId);
                return Ok(passagers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Search passagers");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la recherche");
            }
        }

        [HttpPost("ajouter")]
        public async Task<ActionResult> Create([FromBody] PassagerModel passager)
        {
            try
            {
                passager.Id_Agence = AgenceId;
                var id = await _repository.AddAsync(passager);
                return Ok(new { id, message = "Passager ajouté avec succès !" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Create passager");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de l'ajout");
            }
        }

        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] PassagerModel passager)
        {
            try
            {
                if (id != passager.Id_Passager) return BadRequest("ID incohérent");
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null) return NotFound(new { message = $"Passager ID {id} non trouvé" });
                var message = await _repository.UpdateAsync(passager);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Update passager id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la modification");
            }
        }

        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null) return NotFound(new { message = $"Passager ID {id} non trouvé" });
                var message = await _repository.DeleteAsync(id);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Delete passager id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la suppression");
            }
        }
    }
}
