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

        // GET: api/AffectationVehiculeAgences
        [HttpGet]
        public async Task<ActionResult<List<AffectationVehiculeAgenceModel>>> GetAll()
        {
            var affectations = await _repository.GetAllAsync();
            return Ok(affectations);
        }

        // GET: api/AffectationVehiculeAgences/1
        [HttpGet("{id}")]
        public async Task<ActionResult<AffectationVehiculeAgenceModel>> GetById(int id)
        {
            var affectation = await _repository.GetByIdAsync(id);
            if (affectation == null) return NotFound();
            return Ok(affectation);
        }

        // POST: api/AffectationVehiculeAgences
        [HttpPost]
        public async Task<ActionResult<string>> Create([FromBody] AffectationVehiculeAgenceModel affectation)
        {
            var message = await _repository.AddAsync(affectation);
            return Ok(new { message });
        }

        // PUT: api/AffectationVehiculeAgences/1
        [HttpPut("{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] AffectationVehiculeAgenceModel affectation)
        {
            if (id != affectation.Id_Affectation_Vehicule) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(affectation);
            return Ok(new { message });
        }

        // DELETE: api/AffectationVehiculeAgences/1
        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }
    }
}