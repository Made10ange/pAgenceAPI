using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Controllers;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres;

[Route("api/[controller]")]
[ApiController]
public class CaissierController : AgenceControllerBase
{
    private readonly ICaissierRepository _repo;

    public CaissierController(ICaissierRepository repo) => _repo = repo;

    // GET /api/Caissier/liste
    [HttpGet("liste")]
    public async Task<IActionResult> Liste() => Ok(await _repo.GetAllAsync(AgenceId));

    // GET /api/Caissier/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    // POST /api/Caissier/ajouter
    [HttpPost("ajouter")]
    public async Task<IActionResult> Ajouter([FromBody] CaissierModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(model.MotDePasse))
            return BadRequest("Le mot de passe est obligatoire.");

        var id = await _repo.AjouterAsync(model);
        return id > 0 ? Ok(new { id }) : BadRequest("Login déjà utilisé.");
    }

    // PUT /api/Caissier/modifier
    [HttpPut("modifier")]
    public async Task<IActionResult> Modifier([FromBody] CaissierModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _repo.ModifierAsync(model) ? Ok() : NotFound();
    }

    // PUT /api/Caissier/toggle/{id}
    [HttpPut("toggle/{id:int}")]
    public async Task<IActionResult> Toggle(int id)
        => await _repo.ToggleActifAsync(id) ? Ok() : NotFound();

    // DELETE /api/Caissier/supprimer/{id}
    [HttpDelete("supprimer/{id:int}")]
    public async Task<IActionResult> Supprimer(int id)
        => await _repo.SupprimerAsync(id) ? Ok() : NotFound();
}
