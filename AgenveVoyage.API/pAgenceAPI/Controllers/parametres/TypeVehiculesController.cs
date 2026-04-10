using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;
using System;
using System.Linq;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeVehiculesController : ControllerBase
    {
        private readonly ITypeVehiculeRepository _repository;

        public TypeVehiculesController(ITypeVehiculeRepository repository)
        {
            _repository = repository;
        }

        // ✅ GET: api/TypeVehicules/liste
        [HttpGet("liste")]
        public async Task<ActionResult<List<TypeVehiculeModel>>> GetAll()
        {
            var types = await _repository.GetAllAsync();
            return Ok(types);
        }

        // ✅ GET: api/TypeVehicules/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeVehiculeModel>> GetById(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return Ok(type);
        }

        // ✅ POST: api/TypeVehicules/ajouter
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] TypeVehiculeModel type)
        {
            var message = await _repository.AddAsync(type);
            return Ok(new { message });
        }

        // ✅ PUT: api/TypeVehicules/modifier/{id}
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] TypeVehiculeModel type)
        {
            if (id != type.Id_Type) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(type);
            return Ok(new { message });
        }

        // ✅ DELETE: api/TypeVehicules/supprimer/{id}
        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }

        // ✅ GET: api/TypeVehicules/rechercher?motCle=xxx
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<TypeVehiculeModel>>> Search([FromQuery] string motCle)
        {
            var types = await _repository.GetAllAsync();

            if (!string.IsNullOrEmpty(motCle))
            {
                types = types.Where(t =>
                    t.Libelle_Type.Contains(motCle, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Ok(types);
        }
    }
}