using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;
using System;
using System.Linq;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassagersController : ControllerBase
    {
        private readonly IPassagerRepository _repository;

        public PassagersController(IPassagerRepository repository)
        {
            _repository = repository;
        }

        // ✅ GET: api/Passagers/liste
        [HttpGet("liste")]
        public async Task<ActionResult<List<PassagerModel>>> GetAll()
        {
            var passagers = await _repository.GetAllAsync();
            return Ok(passagers);
        }

        // ✅ GET: api/Passagers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PassagerModel>> GetById(int id)
        {
            var passager = await _repository.GetByIdAsync(id);
            if (passager == null) return NotFound();
            return Ok(passager);
        }

        // ✅ POST: api/Passagers/ajouter (avec photo en base64)
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] PassagerModel passager)
        {
            var message = await _repository.AddAsync(passager);
            return Ok(new { message });
        }

        // ✅ PUT: api/Passagers/modifier/{id}
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] PassagerModel passager)
        {
            if (id != passager.Id_Passager) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(passager);
            return Ok(new { message });
        }

        // ✅ DELETE: api/Passagers/supprimer/{id}
        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }

        // ✅ GET: api/Passagers/rechercher?motCle=xxx
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<PassagerModel>>> Search([FromQuery] string motCle)
        {
            var passagers = await _repository.GetAllAsync();

            if (!string.IsNullOrEmpty(motCle))
            {
                passagers = passagers.Where(p =>
                    p.Nom.Contains(motCle, StringComparison.OrdinalIgnoreCase) ||
                    p.Prenom.Contains(motCle, StringComparison.OrdinalIgnoreCase) ||
                    p.Numero_Piece.Contains(motCle, StringComparison.OrdinalIgnoreCase) ||
                    p.Telephone.Contains(motCle, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Ok(passagers);
        }
    }
}