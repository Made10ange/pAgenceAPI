using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmbarquementVoyagePassagersController : ControllerBase
    {
        private readonly IEmbarquementVoyagePassagerRepository _repository;

        public EmbarquementVoyagePassagersController(IEmbarquementVoyagePassagerRepository repository)
        {
            _repository = repository;
        }

        // GET: api/EmbarquementVoyagePassagers
        [HttpGet]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetAll()
        {
            var embarquements = await _repository.GetAllAsync();
            return Ok(embarquements);
        }

        // GET: api/EmbarquementVoyagePassagers/1
        [HttpGet("{id}")]
        public async Task<ActionResult<EmbarquementVoyagePassagerModel>> GetById(int id)
        {
            var embarquement = await _repository.GetByIdAsync(id);
            if (embarquement == null) return NotFound();
            return Ok(embarquement);
        }

        // POST: api/EmbarquementVoyagePassagers
        [HttpPost]
        public async Task<ActionResult<string>> Create([FromBody] EmbarquementVoyagePassagerModel embarquement)
        {
            var message = await _repository.AddAsync(embarquement);
            return Ok(new { message });
        }

        // PUT: api/EmbarquementVoyagePassagers/1
        [HttpPut("{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] EmbarquementVoyagePassagerModel embarquement)
        {
            if (id != embarquement.Id_Embarquement) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(embarquement);
            return Ok(new { message });
        }

        // DELETE: api/EmbarquementVoyagePassagers/1
        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }
    }
}