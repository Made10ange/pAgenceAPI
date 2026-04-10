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

        // ✅ RECHERCHE
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<AffectationChauffeurAgenceModel>>> Search([FromQuery] string motCle)
        {
            var affectations = await _repository.GetAllAsync();

            if (!string.IsNullOrEmpty(motCle))
            {
                affectations = affectations.Where(a =>
                    a.Id_Chauffeur.ToString().Contains(motCle) ||
                    a.Id_Agence.ToString().Contains(motCle) ||
                    a.Statut?.Contains(motCle, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            return Ok(affectations);
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

        // ✅ GET BY ID (pour modification)
        [HttpGet("{id}")]
        public async Task<ActionResult<AffectationChauffeurAgenceModel>> GetById(int id)
        {
            var affectation = await _repository.GetByIdAsync(id);
            if (affectation == null) return NotFound();
            return Ok(affectation);
        }

        // ============================================
        // ✅ ROUTES STANDARDS REST (compatibilité)
        // ============================================

        [HttpGet]
        public async Task<ActionResult<List<AffectationChauffeurAgenceModel>>> GetAllStandard()
            => await GetAll();

        [HttpPost]
        public async Task<ActionResult<string>> CreateStandard([FromBody] AffectationChauffeurAgenceModel affectation)
            => await Create(affectation);

        [HttpPut("{id}")]
        public async Task<ActionResult<string>> UpdateStandard(int id, [FromBody] AffectationChauffeurAgenceModel affectation)
            => await Update(id, affectation);

        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteStandard(int id)
            => await Delete(id);
    }
}