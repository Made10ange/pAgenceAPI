using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class AffectationChauffeurAgencesController : ControllerBase
    {
        private readonly IAffectationChauffeurAgenceRepository _repository;

        public AffectationChauffeurAgencesController(IAffectationChauffeurAgenceRepository repository)
        {
            _repository = repository;
        }

        // ============================================
        // ✅ ROUTES POUR LE FRONT-END (pAgenceV)
        // ============================================

        // ✅ LISTE DES AFFECTATIONS
        [HttpGet("liste")]
        public async Task<ActionResult<List<AffectationChauffeurAgenceModel>>> GetAll()
        {
            var affectations = await _repository.GetAllAsync();
            return Ok(affectations);
        }

        // ✅ GET PAR ID
        [HttpGet("{id}")]
        public async Task<ActionResult<AffectationChauffeurAgenceModel>> GetById(int id)
        {
            var affectation = await _repository.GetByIdAsync(id);
            if (affectation == null) return NotFound();
            return Ok(affectation);
        }

        // ✅ AJOUTER
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] AffectationChauffeurAgenceModel affectation)
        {
            var message = await _repository.AddAsync(affectation);
            return Ok(new { message });
        }

        // ✅ MODIFIER
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] AffectationChauffeurAgenceModel affectation)
        {
            if (id != affectation.Id_Affectation_Chauffeur) return BadRequest("ID incohérent");
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
