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

        public AgencesController(IAgenceRepository repository)
        {
            _repository = repository;
        }

        // ✅ GET: api/Agences/liste
        [HttpGet("liste")]
        public async Task<ActionResult<List<AgenceModel>>> GetAll()
        {
            var agences = await _repository.GetAllAsync();
            return Ok(agences);
        }

        // ✅ GET: api/Agences/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AgenceModel>> GetById(int id)
        {
            var agence = await _repository.GetByIdAsync(id);
            // ✅ UTILISER 'is null' ET L'OPÉRATEUR '!'
            if (agence is null) return NotFound();
            return Ok(agence!);  // ← Le ! dit au compilateur "c'est pas null ici"
        }

        // ✅ POST: api/Agences/ajouter
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] AgenceModel agence)
        {
            var message = await _repository.AddAsync(agence);
            return Ok(new { message });
        }

        // ✅ PUT: api/Agences/modifier/{id}
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] AgenceModel agence)
        {
            if (id != agence.Id_Agence) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(agence);
            return Ok(new { message });
        }

        // ✅ DELETE: api/Agences/supprimer/{id}
        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }

        // ✅ GET: api/Agences/rechercher?motCle=xxx
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<AgenceModel>>> Search([FromQuery] string motCle)
        {
            var agences = await _repository.GetAllAsync();

            if (!string.IsNullOrEmpty(motCle))
            {
                agences = agences.Where(a =>
                    a.Nom_Agence.Contains(motCle, StringComparison.OrdinalIgnoreCase) ||
                    a.Ville.Contains(motCle, StringComparison.OrdinalIgnoreCase) ||
                    a.Adresse.Contains(motCle, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Ok(agences);
        }
    }
}