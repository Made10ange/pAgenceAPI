using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres;

[Route("api/[controller]")]
[ApiController]
public class ComptesController : ControllerBase
{
    private readonly ICompteRepository _repo;

    public ComptesController(ICompteRepository repo)
    {
        _repo = repo;
    }

    // GET /api/Comptes/liste
    [HttpGet("liste")]
    public async Task<IActionResult> Liste() => Ok(await _repo.GetAllAsync());

    // GET /api/Comptes/{numcompte}
    [HttpGet("{numcompte}")]
    public async Task<IActionResult> GetByNum(string numcompte)
    {
        var item = await _repo.GetByNumAsync(numcompte);
        return item == null ? NotFound() : Ok(item);
    }

    // POST /api/Comptes/ajouter
    [HttpPost("ajouter")]
    public async Task<IActionResult> Ajouter([FromBody] CompteModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _repo.AddAsync(model)
            ? Ok()
            : BadRequest("Numéro de compte déjà utilisé.");
    }

    // PUT /api/Comptes/modifier
    [HttpPut("modifier")]
    public async Task<IActionResult> Modifier([FromBody] CompteModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _repo.UpdateAsync(model)
            ? Ok()
            : BadRequest("Modification impossible (compte inexistant ou référence circulaire).");
    }

    // DELETE /api/Comptes/supprimer/{numcompte}
    [HttpDelete("supprimer/{numcompte}")]
    public async Task<IActionResult> Supprimer(string numcompte)
    {
        return await _repo.DeleteAsync(numcompte)
            ? Ok()
            : BadRequest("Suppression impossible — ce compte a des sous-comptes.");
    }
}
