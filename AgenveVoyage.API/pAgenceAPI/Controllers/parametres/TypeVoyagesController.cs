using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;
using System;
using System.Linq;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeVoyagesController : ControllerBase
    {
        private readonly ITypeVoyageRepository _repository;

        public TypeVoyagesController(ITypeVoyageRepository repository)
        {
            _repository = repository;
        }

        // ✅ GET: api/TypeVoyages/liste
        [HttpGet("liste")]
        public async Task<ActionResult<List<TypeVoyageModel>>> GetAll()
        {
            var types = await _repository.GetAllAsync();
            return Ok(types);
        }

        // ✅ GET: api/TypeVoyages/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeVoyageModel>> GetById(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return Ok(type);
        }

        // ✅ POST: api/TypeVoyages/ajouter
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] TypeVoyageModel type)
        {
            var message = await _repository.AddAsync(type);
            return Ok(new { message });
        }

        // ✅ PUT: api/TypeVoyages/modifier/{id}
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] TypeVoyageModel type)
        {
            if (id != type.Id_Type_Voyage) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(type);
            return Ok(new { message });
        }

        // ✅ DELETE: api/TypeVoyages/supprimer/{id}
        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }

        // ✅ GET: api/TypeVoyages/rechercher?motCle=xxx
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<TypeVoyageModel>>> Search([FromQuery] string motCle)
        {
            var types = await _repository.GetAllAsync();

            if (!string.IsNullOrEmpty(motCle))
            {
                types = types.Where(t =>
                    t.Libelle_Type_Voyage.Contains(motCle, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Ok(types);
        }
    }
}