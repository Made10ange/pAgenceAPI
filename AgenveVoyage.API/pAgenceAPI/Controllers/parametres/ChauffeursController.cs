using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;
using System;
using System.Linq;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChauffeursController : ControllerBase
    {
        private readonly IChauffeurRepository _repository;

        public ChauffeursController(IChauffeurRepository repository)
        {
            _repository = repository;
        }

        // ✅ GET: api/Chauffeurs/liste
        [HttpGet("liste")]
        public async Task<ActionResult<List<ChauffeurModel>>> GetAll()
        {
            var chauffeurs = await _repository.GetAllAsync();
            return Ok(chauffeurs);
        }

        // ✅ GET: api/Chauffeurs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ChauffeurModel>> GetById(int id)
        {
            var chauffeur = await _repository.GetByIdAsync(id);
            if (chauffeur == null) return NotFound();
            return Ok(chauffeur);
        }

        // ✅ POST: api/Chauffeurs/ajouter (avec photo en base64)
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] ChauffeurModel chauffeur)
        {
            var message = await _repository.AddAsync(chauffeur);
            return Ok(new { message });
        }

        // ✅ PUT: api/Chauffeurs/modifier/{id}
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] ChauffeurModel chauffeur)
        {
            if (id != chauffeur.Id_Chauffeur) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(chauffeur);
            return Ok(new { message });
        }

        // ✅ DELETE: api/Chauffeurs/supprimer/{id}
        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }

        // ✅ GET: api/Chauffeurs/rechercher?motCle=xxx
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<ChauffeurModel>>> Search([FromQuery] string motCle)
        {
            var chauffeurs = await _repository.GetAllAsync();

            if (!string.IsNullOrEmpty(motCle))
            {
                chauffeurs = chauffeurs.Where(c =>
                    c.Nom.Contains(motCle, StringComparison.OrdinalIgnoreCase) ||
                    c.Prenom.Contains(motCle, StringComparison.OrdinalIgnoreCase) ||
                    c.Numero_Piece.Contains(motCle, StringComparison.OrdinalIgnoreCase) ||
                    c.Telephone.Contains(motCle, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Ok(chauffeurs);
        }
    }
}