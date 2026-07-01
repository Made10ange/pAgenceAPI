using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.personnel;

[ApiController]
[Route("api/[controller]")]
public class PostesController : ControllerBase
{
    private readonly IPosteRepository _repo;
    public PostesController(IPosteRepository repo) => _repo = repo;

    [HttpGet("liste")]
    public async Task<IActionResult> Liste() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("ajouter")]
    public async Task<IActionResult> Ajouter([FromBody] PosteModel poste)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var id = await _repo.AddAsync(poste);
        return Ok(new { id });
    }

    [HttpPut("modifier")]
    public async Task<IActionResult> Modifier([FromBody] PosteModel poste)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _repo.UpdateAsync(poste) ? Ok() : NotFound();
    }

    [HttpDelete("supprimer/{id}")]
    public async Task<IActionResult> Supprimer(int id)
        => await _repo.DeleteAsync(id) ? Ok() : NotFound();
}
