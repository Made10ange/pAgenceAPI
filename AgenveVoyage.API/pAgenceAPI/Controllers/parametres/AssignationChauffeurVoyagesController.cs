using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignationChauffeurVoyagesController : ControllerBase
    {
        private readonly IAssignationChauffeurVoyageRepository _repository;

        public AssignationChauffeurVoyagesController(IAssignationChauffeurVoyageRepository repository)
        {
            _repository = repository;
        }

        // GET: api/AssignationChauffeurVoyages
        [HttpGet]
        public async Task<ActionResult<List<AssignationChauffeurVoyageModel>>> GetAll()
        {
            var assignations = await _repository.GetAllAsync();
            return Ok(assignations);
        }

        // GET: api/AssignationChauffeurVoyages/1
        [HttpGet("{id}")]
        public async Task<ActionResult<AssignationChauffeurVoyageModel>> GetById(int id)
        {
            var assignation = await _repository.GetByIdAsync(id);
            if (assignation == null) return NotFound();
            return Ok(assignation);
        }

        // POST: api/AssignationChauffeurVoyages
        [HttpPost]
        public async Task<ActionResult<string>> Create([FromBody] AssignationChauffeurVoyageModel assignation)
        {
            var message = await _repository.AddAsync(assignation);
            return Ok(new { message });
        }

        // PUT: api/AssignationChauffeurVoyages/1
        [HttpPut("{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] AssignationChauffeurVoyageModel assignation)
        {
            if (id != assignation.Id_Assignation) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(assignation);
            return Ok(new { message });
        }

        // DELETE: api/AssignationChauffeurVoyages/1
        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }
    }
}