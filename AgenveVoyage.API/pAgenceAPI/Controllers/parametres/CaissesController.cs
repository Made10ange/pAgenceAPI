using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Controllers;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres;

[Route("api/[controller]")]
[ApiController]
public class CaissesController : AgenceControllerBase
{
    private readonly ICaisseRepository _repo;

    public CaissesController(ICaisseRepository repo) => _repo = repo;

    // GET /api/Caisses/liste
    [HttpGet("liste")]
    public async Task<IActionResult> Liste() => Ok(await _repo.GetAllAsync(AgenceId));

    // GET /api/Caisses/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    // POST /api/Caisses/ajouter
    [HttpPost("ajouter")]
    public async Task<IActionResult> Ajouter([FromBody] CaisseModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var id = await _repo.AjouterAsync(model);
        return id > 0 ? Ok(new { id }) : BadRequest("Erreur lors de la création.");
    }

    // PUT /api/Caisses/modifier
    [HttpPut("modifier")]
    public async Task<IActionResult> Modifier([FromBody] CaisseModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _repo.ModifierAsync(model) ? Ok() : NotFound();
    }

    // DELETE /api/Caisses/supprimer/{id}
    [HttpDelete("supprimer/{id:int}")]
    public async Task<IActionResult> Supprimer(int id)
        => await _repo.SupprimerAsync(id)
            ? Ok()
            : BadRequest("Suppression impossible — des caissiers sont encore affectés à cette caisse.");

    // ─── Affectations ──────────────────────────────────────────────────────

    // GET /api/Caisses/affectations?idCaisse=1
    [HttpGet("affectations")]
    public async Task<IActionResult> Affectations(int? idCaisse = null)
        => Ok(await _repo.GetAffectationsAsync(idCaisse));

    // POST /api/Caisses/affecter
    [HttpPost("affecter")]
    public async Task<IActionResult> Affecter([FromBody] AffectationCaissierModel model)
        => await _repo.AffecterAsync(model) ? Ok() : BadRequest("Affectation impossible.");

    // PUT /api/Caisses/desaffecter/{id}
    [HttpPut("desaffecter/{id:int}")]
    public async Task<IActionResult> Desaffecter(int id)
        => await _repo.DesaffecterAsync(id) ? Ok() : NotFound();
}
