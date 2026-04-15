using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class AffectationVehiculeAgencesController : ControllerBase
    {
        private readonly IAffectationVehiculeAgenceRepository _repository;

        public AffectationVehiculeAgencesController(IAffectationVehiculeAgenceRepository repository)
        {
            _repository = repository;
        }

        // ✅ LISTE DES AFFECTATIONS
        [HttpGet("liste")]
        public async Task<ActionResult<List<AffectationVehiculeAgenceModel>>> GetAll()
        {
            var affectations = await _repository.GetAllAsync();
            return Ok(affectations);
        }

        // ✅ GET PAR ID
        [HttpGet("{id}")]
        public async Task<ActionResult<AffectationVehiculeAgenceModel>> GetById(int id)
        {
            var affectation = await _repository.GetByIdAsync(id);
            if (affectation == null)
            {
                return NotFound();
            }

            return Ok(affectation);
        }

        // ✅ RECHERCHE
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<AffectationVehiculeAgenceModel>>> Search([FromQuery] string motCle)
        {
            if (string.IsNullOrWhiteSpace(motCle))
            {
                var all = await _repository.GetAllAsync();
                return Ok(all);
            }

            var affectations = await _repository.SearchAsync(motCle.Trim());
            return Ok(affectations);
        }

        // ✅ AJOUTER
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] AffectationVehiculeAgenceModel affectation)
        {
            var message = await _repository.AddAsync(affectation);
            return Ok(new { message });
        }

        // ✅ MODIFIER
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] AffectationVehiculeAgenceModel affectation)
        {
            if (id != affectation.Id_Affectation_Vehicule) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(affectation);
            return Ok(new { message });
        }

        // ✅ SUPPRIMER
        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }
    }
}
