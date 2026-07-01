using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres;

[Route("api/[controller]")]
[ApiController]
public class UtilisateursController : ControllerBase
{
    private readonly IUtilisateurRepository _repo;

    public UtilisateursController(IUtilisateurRepository repo) => _repo = repo;

    [HttpGet("liste")]
    public async Task<ActionResult<IEnumerable<UtilisateurModel>>> Liste()
        => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<UtilisateurModel>> GetById(int id)
    {
        var agent = await _repo.GetByIdAsync(id);
        return agent == null ? NotFound() : Ok(agent);
    }

    [HttpPost("ajouter")]
    public async Task<ActionResult> Ajouter([FromBody] UtilisateurModel agent)
    {
        if (string.IsNullOrWhiteSpace(agent.MotDePasse))
            return BadRequest("Mot de passe requis.");
        var id = await _repo.AddAsync(agent);
        return Ok(new { Id = id });
    }

    [HttpPut("modifier/{id}")]
    public async Task<ActionResult> Modifier(int id, [FromBody] UtilisateurModel agent)
    {
        agent.Id_Utilisateur = id;
        var ok = await _repo.UpdateAsync(agent);
        return ok ? Ok() : NotFound();
    }

    [HttpPut("modifier-mdp/{id}")]
    public async Task<ActionResult> ModifierMotDePasse(int id, [FromBody] ChangerMotDePasseRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NouveauMotDePasse))
            return BadRequest("Mot de passe requis.");
        var hash = BCrypt.Net.BCrypt.HashPassword(req.NouveauMotDePasse);
        var ok = await _repo.UpdatePasswordAsync(id, hash);
        return ok ? Ok() : NotFound();
    }

    [HttpDelete("supprimer/{id}")]
    public async Task<ActionResult> Supprimer(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? Ok() : NotFound();
    }
}

public class ChangerMotDePasseRequest
{
    public string NouveauMotDePasse { get; set; } = "";
}

