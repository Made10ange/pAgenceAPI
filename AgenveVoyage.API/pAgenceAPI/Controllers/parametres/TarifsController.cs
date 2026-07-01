using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarifsController : ControllerBase
    {
        private readonly ITarifRepository _repo;

        public TarifsController(ITarifRepository repo) => _repo = repo;

        [HttpGet("liste")]
        public async Task<IActionResult> Liste() => Ok(await _repo.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _repo.GetByIdAsync(id);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpGet("rechercher")]
        public async Task<IActionResult> Rechercher(
            [FromQuery] int? idTypeVoyage,
            [FromQuery] string? depart,
            [FromQuery] string? arrivee,
            [FromQuery] string? typePassager)
        {
            return Ok(await _repo.RechercherAsync(idTypeVoyage, depart, arrivee, typePassager));
        }

        [HttpPost("ajouter")]
        public async Task<IActionResult> Ajouter([FromBody] TarifModel tarif)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return await _repo.AddAsync(tarif) ? Ok() : StatusCode(500);
        }

        [HttpPut("modifier/{id}")]
        public async Task<IActionResult> Modifier(int id, [FromBody] TarifModel tarif)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            tarif.Id_Tarif = id;
            return await _repo.UpdateAsync(tarif) ? Ok() : NotFound();
        }

        [HttpDelete("supprimer/{id}")]
        public async Task<IActionResult> Supprimer(int id)
            => await _repo.DeleteAsync(id) ? Ok() : NotFound();
    }
}
