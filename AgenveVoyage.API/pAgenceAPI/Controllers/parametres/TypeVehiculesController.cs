using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeVehiculesController : AgenceControllerBase
    {
        private readonly ITypeVehiculeRepository _repository;
        private readonly ILogger<TypeVehiculesController> _logger;

        public TypeVehiculesController(ITypeVehiculeRepository repository, ILogger<TypeVehiculesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("liste")]
        public async Task<ActionResult<List<TypeVehiculeModel>>> GetAll()
        {
            try
            {
                return Ok(await _repository.GetAllAsync(AgenceId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAll type véhicules");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du chargement");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TypeVehiculeModel>> GetById(int id)
        {
            try
            {
                var type = await _repository.GetByIdAsync(id);
                if (type == null) return NotFound();
                return Ok(type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetById type véhicule id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la récupération");
            }
        }

        [HttpGet("rechercher")]
        public async Task<ActionResult<List<TypeVehiculeModel>>> Search([FromQuery] string motCle)
        {
            try
            {
                var types = string.IsNullOrWhiteSpace(motCle)
                    ? await _repository.GetAllAsync(AgenceId)
                    : await _repository.SearchAsync(motCle, AgenceId);
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Search type véhicules");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la recherche");
            }
        }

        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] TypeVehiculeModel type)
        {
            try
            {
                type.Id_Agence = AgenceId;
                var message = await _repository.AddAsync(type);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Create type véhicule");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de l'ajout");
            }
        }

        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] TypeVehiculeModel type)
        {
            try
            {
                if (id != type.Id_Type) return BadRequest("ID incohérent");
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null) return NotFound(new { message = $"Type véhicule ID {id} non trouvé" });
                var message = await _repository.UpdateAsync(type);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Update type véhicule id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la modification");
            }
        }

        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null) return NotFound(new { message = $"Type véhicule ID {id} non trouvé" });
                var message = await _repository.DeleteAsync(id);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Delete type véhicule id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la suppression");
            }
        }
    }
}
