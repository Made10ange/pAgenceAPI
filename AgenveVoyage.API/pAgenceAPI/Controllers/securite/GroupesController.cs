using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.securite;

[ApiController]
[Route("api/[controller]")]
public class GroupesController : ControllerBase
{
    private readonly IGroupeRepository _repo;
    public GroupesController(IGroupeRepository repo) => _repo = repo;

    [HttpGet("liste")]
    public async Task<IActionResult> Liste() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var g = await _repo.GetByIdAsync(id);
        return g is null ? NotFound() : Ok(g);
    }

    [HttpPost("ajouter")]
    public async Task<IActionResult> Ajouter([FromBody] GroupeModel groupe)
    {
        var id = await _repo.AddAsync(groupe);
        return Ok(new { id });
    }

    [HttpPut("modifier/{id}")]
    public async Task<IActionResult> Modifier(int id, [FromBody] GroupeModel groupe)
    {
        var ok = await _repo.UpdateAsync(id, groupe);
        return ok ? Ok() : NotFound();
    }

    [HttpDelete("supprimer/{id}")]
    public async Task<IActionResult> Supprimer(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? Ok() : NotFound();
    }

    [HttpGet("{id}/agents")]
    public async Task<IActionResult> GetAgents(int id) =>
        Ok(await _repo.GetAgentsAsync(id));

    [HttpPost("{id}/affecter/{idAgent}")]
    public async Task<IActionResult> AffecterAgent(int id, int idAgent)
    {
        await _repo.AffecterAgentAsync(id, idAgent);
        return Ok();
    }

    [HttpDelete("{idGroupe}/retirer/{idAgent}")]
    public async Task<IActionResult> RetirerAgent(int idGroupe, int idAgent)
    {
        var ok = await _repo.RetirerAgentAsync(idGroupe, idAgent);
        return ok ? Ok() : NotFound();
    }
}
